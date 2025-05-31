using System.Linq.Expressions;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Services;

public class MovieServiceTests
{
    private readonly Mock<ILogger<MovieService>> _mockLogger;
    private readonly Mock<IRepository<Movie>> _mockMovieRepository;
    private readonly Mock<IStarWarsApiService> _mockStarWarsApiService;
    private readonly MovieService _movieService;

    public MovieServiceTests()
    {
        _mockMovieRepository = new Mock<IRepository<Movie>>();
        _mockStarWarsApiService = new Mock<IStarWarsApiService>();
        _mockLogger = new Mock<ILogger<MovieService>>();

        _movieService = new MovieService(
            _mockMovieRepository.Object,
            _mockStarWarsApiService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ShouldReturnAllMovies()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new() { Id = 1, Title = "A New Hope", EpisodeId = 4 },
            new() { Id = 2, Title = "The Empire Strikes Back", EpisodeId = 5 }
        };

        _mockMovieRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(movies);

        // Act
        var result = await _movieService.GetAllMoviesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("A New Hope", result.First().Title);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithValidId_ShouldReturnMovie()
    {
        // Arrange
        var movie = new Movie { Id = 1, Title = "A New Hope", EpisodeId = 4 };
        _mockMovieRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(movie);

        // Act
        var result = await _movieService.GetMovieByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A New Hope", result.Title);
        Assert.Equal(4, result.EpisodeId);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockMovieRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await _movieService.GetMovieByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateMovieAsync_WithValidData_ShouldCreateMovie()
    {
        // Arrange
        var createMovieDto = new CreateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        _mockMovieRepository.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync((Movie?)null);

        _mockMovieRepository.Setup(r => r.AddAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) => m);

        // Act
        var result = await _movieService.CreateMovieAsync(createMovieDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A New Hope", result.Title);
        Assert.Equal(4, result.EpisodeId);
        _mockMovieRepository.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task CreateMovieAsync_WithDuplicateEpisodeId_ShouldThrowException()
    {
        // Arrange
        var createMovieDto = new CreateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        var existingMovie = new Movie { Id = 1, EpisodeId = 4 };
        _mockMovieRepository.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(existingMovie);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _movieService.CreateMovieAsync(createMovieDto));

        Assert.Contains("Ya existe una película con el Episode ID 4", exception.Message);
    }

    [Fact]
    public async Task DeleteMovieAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockMovieRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _movieService.DeleteMovieAsync(1);

        // Assert
        Assert.True(result);
        _mockMovieRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteMovieAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _mockMovieRepository.Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _movieService.DeleteMovieAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SyncMoviesFromStarWarsApiAsync_ShouldSyncNewMovies()
    {
        // Arrange
        var apiResponse = new StarWarsApiResponse
        {
            Results = new List<StarWarsFilmResult>
            {
                new()
                {
                    Properties = new StarWarsFilmProperties
                    {
                        Title = "A New Hope",
                        Episode_id = 4,
                        Director = "George Lucas",
                        Producer = "Gary Kurtz",
                        Opening_crawl = "It is a period of civil war...",
                        Release_date = "1977-05-25"
                    }
                }
            }
        };

        _mockStarWarsApiService.Setup(s => s.GetFilmsAsync())
            .ReturnsAsync(apiResponse);

        _mockMovieRepository.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync((Movie?)null);

        _mockMovieRepository.Setup(r => r.AddAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) => m);

        // Act
        var result = await _movieService.SyncMoviesFromStarWarsApiAsync();

        // Assert
        Assert.Equal(1, result);
        _mockMovieRepository.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Once);
    }
}