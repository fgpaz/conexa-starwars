using System.Security.Claims;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Mock UserManager
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Mock RoleManager
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null!, null!, null!, null!);

        // Mock Configuration
        _mockConfiguration = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("ConexaStarWarsSecretKeyForJWTTokenGeneration2024!");
        jwtSection.Setup(x => x["Issuer"]).Returns("ConexaStarWarsAPI");
        jwtSection.Setup(x => x["Audience"]).Returns("ConexaStarWarsClient");
        jwtSection.Setup(x => x["ExpirationHours"]).Returns("24");
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _authService = new AuthService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var user = new User
        {
            Id = "user-id",
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "RegularUser"))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "RegularUser" });

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registerDto.Email, result.Email);
        Assert.Equal(registerDto.FirstName, result.FirstName);
        Assert.Equal(registerDto.LastName, result.LastName);
        Assert.Contains("RegularUser", result.Roles);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var existingUser = new User { Email = registerDto.Email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(registerDto));

        Assert.Contains("El usuario ya existe con este email", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var user = new User
        {
            Id = "user-id",
            Email = loginDto.Email,
            UserName = loginDto.Email,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "RegularUser" });

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loginDto.Email, result.Email);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Contains("RegularUser", result.Roles);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Test123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));

        Assert.Equal("Credenciales inválidas", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = "user-id",
            Email = loginDto.Email,
            UserName = loginDto.Email,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));

        Assert.Equal("Credenciales inválidas", exception.Message);
    }

    [Fact]
    public async Task CreateRoleAsync_WithNewRole_ShouldReturnTrue()
    {
        // Arrange
        var roleName = "TestRole";

        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(false);

        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.CreateRoleAsync(roleName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateRoleAsync_WithExistingRole_ShouldReturnFalse()
    {
        // Arrange
        var roleName = "ExistingRole";

        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.CreateRoleAsync(roleName);

        // Assert
        Assert.False(result);
    }
} 