using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Endpoints;

public class MoviesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IMediator> _mockMediator;

    public MoviesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _mockMediator = new Mock<IMediator>();
        _mockAuthService = new Mock<IAuthService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover servicios reales y agregar mocks
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null) services.Remove(mediatorDescriptor);
                services.AddScoped(_ => _mockMediator.Object);

                var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
                if (authDescriptor != null) services.Remove(authDescriptor);
                services.AddScoped(_ => _mockAuthService.Object);
            });
        });

        _client = _factory.CreateClient();
    }

    private void SetupAuthenticatedUser(string role = "RegularUser")
    {
        // Configurar un token JWT falso para las pruebas
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-jwt-token");
    }

    [Fact]
    public async Task GetAllMovies_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/movies");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllMovies_WithAuth_ShouldReturnOk()
    {
        // Arrange
        SetupAuthenticatedUser();

        var movies = new List<MovieDto>
        {
            new() { Id = 1, Title = "A New Hope", EpisodeId = 4 },
            new() { Id = 2, Title = "The Empire Strikes Back", EpisodeId = 5 }
        };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetAllMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        // Act
        var response = await _client.GetAsync("/api/movies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<MovieDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMovieById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        SetupAuthenticatedUser();

        var movie = new MovieDto { Id = 1, Title = "A New Hope", EpisodeId = 4 };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetMovieByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        // Act
        var response = await _client.GetAsync("/api/movies/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MovieDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal("A New Hope", result.Title);
    }

    [Fact]
    public async Task GetMovieById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedUser();

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetMovieByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieDto?)null);

        // Act
        var response = await _client.GetAsync("/api/movies/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        var createMovieDto = new CreateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        var createdMovie = new MovieDto
        {
            Id = 1,
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<CreateMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMovie);

        // Act
        var response = await _client.PostAsJsonAsync("/api/movies", createMovieDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MovieDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal("A New Hope", result.Title);
    }

    [Fact]
    public async Task CreateMovie_WithDuplicateEpisodeId_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        var createMovieDto = new CreateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<CreateMovieCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ya existe una película con el Episode ID 4"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/movies", createMovieDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ya existe una película con el Episode ID 4", content);
    }

    [Fact]
    public async Task UpdateMovie_WithValidData_ShouldReturnOk()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        var updateMovieDto = new UpdateMovieDto
        {
            Title = "A New Hope - Updated",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        var updatedMovie = new MovieDto
        {
            Id = 1,
            Title = "A New Hope - Updated",
            EpisodeId = 4
        };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<UpdateMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedMovie);

        // Act
        var response = await _client.PutAsJsonAsync("/api/movies/1", updateMovieDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MovieDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal("A New Hope - Updated", result.Title);
    }

    [Fact]
    public async Task UpdateMovie_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        var updateMovieDto = new UpdateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<UpdateMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieDto?)null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/movies/999", updateMovieDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMovie_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<DeleteMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync("/api/movies/1");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMovie_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<DeleteMovieCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync("/api/movies/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SyncMovies_ShouldReturnOk()
    {
        // Arrange
        SetupAuthenticatedUser("Administrator");

        _mockMediator.Setup(m => m.SendAsync(It.IsAny<SyncMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        // Act
        var response = await _client.PostAsync("/api/movies/sync", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sincronización completada exitosamente", content);
        Assert.Contains("6", content);
    }
}