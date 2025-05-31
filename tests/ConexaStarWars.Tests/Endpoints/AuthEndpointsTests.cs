using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _mockAuthService = new Mock<IAuthService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover el servicio real y agregar el mock
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
                if (descriptor != null) services.Remove(descriptor);
                services.AddScoped(_ => _mockAuthService.Object);

                // Configurar autenticación de prueba
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
                    
                // Configurar autorización sin roles para simplificar tests
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "fake-jwt-token",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { "RegularUser" },
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(authResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("fake-jwt-token", result.Token);
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email", // Email inválido
            Password = "123", // Password muy corto
            ConfirmPassword = "456", // Passwords no coinciden
            FirstName = "",
            LastName = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new InvalidOperationException("El email ya está registrado"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("El email ya está registrado", content);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "fake-jwt-token",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { "RegularUser" },
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(authResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("fake-jwt-token", result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Credenciales inválidas"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "", // Email vacío
            Password = "" // Password vacío
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithServerError_ShouldReturnInternalServerError()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new Exception("Error interno"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithServerError_ShouldReturnInternalServerError()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new Exception("Error interno"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}