using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Handlers.Commands;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Handlers;

public class CommandHandlersTests
{
    private readonly Mock<IRepository<Movie>> _mockMovieRepository;
    private readonly Mock<IStarWarsApiService> _mockStarWarsApiService;
    private readonly Mock<ILogger<CreateMovieCommandHandler>> _mockCreateLogger;
    private readonly Mock<ILogger<UpdateMovieCommandHandler>> _mockUpdateLogger;
    private readonly Mock<ILogger<DeleteMovieCommandHandler>> _mockDeleteLogger;
    private readonly Mock<ILogger<SyncMoviesCommandHandler>> _mockSyncLogger;

    public CommandHandlersTests()
    {
        _mockMovieRepository = new Mock<IRepository<Movie>>();
        _mockStarWarsApiService = new Mock<IStarWarsApiService>();
        _mockCreateLogger = new Mock<ILogger<CreateMovieCommandHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateMovieCommandHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteMovieCommandHandler>>();
        _mockSyncLogger = new Mock<ILogger<SyncMoviesCommandHandler>>();
    }

    #region CreateMovieCommandHandler Tests

    [Fact]
    public async Task CreateMovieCommandHandler_WithValidData_ShouldCreateMovie()
    {
        // Arrange
        var handler = new CreateMovieCommandHandler(_mockMovieRepository.Object, _mockCreateLogger.Object);
        
        var command = new CreateMovieCommand
        {
            MovieData = new CreateMovieDto
            {
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25),
                Characters = new List<string> { "Luke Skywalker", "Princess Leia" },
                Planets = new List<string> { "Tatooine", "Alderaan" },
                Starships = new List<string> { "Death Star", "Millennium Falcon" },
                Vehicles = new List<string> { "Sandcrawler" },
                Species = new List<string> { "Human", "Droid" }
            },
            UserId = "user-123"
        };

        var createdMovie = new Movie
        {
            Id = 1,
            Title = command.MovieData.Title,
            EpisodeId = command.MovieData.EpisodeId,
            Director = command.MovieData.Director,
            Producer = command.MovieData.Producer,
            OpeningCrawl = command.MovieData.OpeningCrawl,
            ReleaseDate = command.MovieData.ReleaseDate,
            Characters = command.MovieData.Characters,
            Planets = command.MovieData.Planets,
            Starships = command.MovieData.Starships,
            Vehicles = command.MovieData.Vehicles,
            Species = command.MovieData.Species,
            CreatedAt = DateTime.UtcNow
        };

        _mockMovieRepository.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Movie, bool>>>()))
            .ReturnsAsync((Movie?)null);

        _mockMovieRepository.Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .ReturnsAsync(createdMovie);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.MovieData.Title, result.Title);
        Assert.Equal(command.MovieData.EpisodeId, result.EpisodeId);
        Assert.Equal(command.MovieData.Director, result.Director);

        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task CreateMovieCommandHandler_WithDuplicateEpisodeId_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateMovieCommandHandler(_mockMovieRepository.Object, _mockCreateLogger.Object);
        
        var command = new CreateMovieCommand
        {
            MovieData = new CreateMovieDto
            {
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25)
            },
            UserId = "user-123"
        };

        var existingMovie = new Movie { Id = 1, EpisodeId = 4, Title = "Existing Movie" };

        _mockMovieRepository.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Movie, bool>>>()))
            .ReturnsAsync(existingMovie);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));

        Assert.Contains("Ya existe una película con el Episode ID 4", exception.Message);
        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Never);
    }

    #endregion

    #region UpdateMovieCommandHandler Tests

    [Fact]
    public async Task UpdateMovieCommandHandler_WithValidData_ShouldUpdateMovie()
    {
        // Arrange
        var handler = new UpdateMovieCommandHandler(_mockMovieRepository.Object, _mockUpdateLogger.Object);
        
        var command = new UpdateMovieCommand
        {
            MovieId = 1,
            MovieData = new UpdateMovieDto
            {
                Title = "A New Hope - Updated",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz, Rick McCallum",
                OpeningCrawl = "Updated opening crawl...",
                ReleaseDate = new DateTime(1977, 5, 25)
            },
            UserId = "user-123"
        };

        var existingMovie = new Movie
        {
            Id = 1,
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz",
            OpeningCrawl = "Original opening crawl...",
            ReleaseDate = new DateTime(1977, 5, 25),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedMovie = new Movie
        {
            Id = 1,
            Title = command.MovieData.Title,
            EpisodeId = command.MovieData.EpisodeId,
            Director = command.MovieData.Director,
            Producer = command.MovieData.Producer,
            OpeningCrawl = command.MovieData.OpeningCrawl,
            ReleaseDate = command.MovieData.ReleaseDate,
            CreatedAt = existingMovie.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(command.MovieId))
            .ReturnsAsync(existingMovie);

        _mockMovieRepository.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Movie, bool>>>()))
            .ReturnsAsync((Movie?)null);

        _mockMovieRepository.Setup(x => x.UpdateAsync(It.IsAny<Movie>()))
            .ReturnsAsync(updatedMovie);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.MovieData.Title, result.Title);
        Assert.Equal(command.MovieData.Producer, result.Producer);

        _mockMovieRepository.Verify(x => x.UpdateAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMovieCommandHandler_WithNonExistentMovie_ShouldReturnNull()
    {
        // Arrange
        var handler = new UpdateMovieCommandHandler(_mockMovieRepository.Object, _mockUpdateLogger.Object);
        
        var command = new UpdateMovieCommand
        {
            MovieId = 999,
            MovieData = new UpdateMovieDto
            {
                Title = "Non-existent Movie",
                EpisodeId = 999
            },
            UserId = "user-123"
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(command.MovieId))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Null(result);
        _mockMovieRepository.Verify(x => x.UpdateAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMovieCommandHandler_WithDuplicateEpisodeId_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateMovieCommandHandler(_mockMovieRepository.Object, _mockUpdateLogger.Object);
        
        var command = new UpdateMovieCommand
        {
            MovieId = 1,
            MovieData = new UpdateMovieDto
            {
                Title = "A New Hope",
                EpisodeId = 5 // Trying to change to episode 5
            },
            UserId = "user-123"
        };

        var existingMovie = new Movie { Id = 1, EpisodeId = 4, Title = "A New Hope" };
        var conflictingMovie = new Movie { Id = 2, EpisodeId = 5, Title = "Empire Strikes Back" };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(command.MovieId))
            .ReturnsAsync(existingMovie);

        _mockMovieRepository.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Movie, bool>>>()))
            .ReturnsAsync(conflictingMovie);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));

        Assert.Contains("Ya existe otra película con el Episode ID 5", exception.Message);
        _mockMovieRepository.Verify(x => x.UpdateAsync(It.IsAny<Movie>()), Times.Never);
    }

    #endregion

    #region DeleteMovieCommandHandler Tests

    [Fact]
    public async Task DeleteMovieCommandHandler_WithValidId_ShouldDeleteMovie()
    {
        // Arrange
        var handler = new DeleteMovieCommandHandler(_mockMovieRepository.Object, _mockDeleteLogger.Object);
        
        var command = new DeleteMovieCommand
        {
            MovieId = 1,
            UserId = "user-123"
        };

        _mockMovieRepository.Setup(x => x.DeleteAsync(command.MovieId))
            .ReturnsAsync(true);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result);
        _mockMovieRepository.Verify(x => x.DeleteAsync(command.MovieId), Times.Once);
    }

    [Fact]
    public async Task DeleteMovieCommandHandler_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var handler = new DeleteMovieCommandHandler(_mockMovieRepository.Object, _mockDeleteLogger.Object);
        
        var command = new DeleteMovieCommand
        {
            MovieId = 999,
            UserId = "user-123"
        };

        _mockMovieRepository.Setup(x => x.DeleteAsync(command.MovieId))
            .ReturnsAsync(false);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result);
        _mockMovieRepository.Verify(x => x.DeleteAsync(command.MovieId), Times.Once);
    }

    #endregion

    #region SyncMoviesCommandHandler Tests

    [Fact]
    public async Task SyncMoviesCommandHandler_WithNewMovies_ShouldSyncSuccessfully()
    {
        // Arrange
        var handler = new SyncMoviesCommandHandler(
            _mockMovieRepository.Object, 
            _mockStarWarsApiService.Object, 
            _mockSyncLogger.Object);
        
        var command = new SyncMoviesCommand { UserId = "admin-123" };

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
                        Release_date = "1977-05-25",
                        Characters = new List<string> { "Luke Skywalker" },
                        Planets = new List<string> { "Tatooine" },
                        Starships = new List<string> { "Death Star" },
                        Vehicles = new List<string> { "Sandcrawler" },
                        Species = new List<string> { "Human" },
                        Url = "https://swapi.tech/api/films/1"
                    }
                }
            }
        };

        _mockStarWarsApiService.Setup(x => x.GetFilmsAsync())
            .ReturnsAsync(apiResponse);

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Movie>());

        _mockMovieRepository.Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie movie) => movie);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(1, result);
        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task SyncMoviesCommandHandler_WithExistingMovies_ShouldSkipDuplicates()
    {
        // Arrange
        var handler = new SyncMoviesCommandHandler(
            _mockMovieRepository.Object, 
            _mockStarWarsApiService.Object, 
            _mockSyncLogger.Object);
        
        var command = new SyncMoviesCommand { UserId = "admin-123" };

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

        var existingMovies = new List<Movie>
        {
            new()
            {
                Id = 1,
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25)
            }
        };

        _mockStarWarsApiService.Setup(x => x.GetFilmsAsync())
            .ReturnsAsync(apiResponse);

        _mockMovieRepository.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Movie, bool>>>()))
            .ReturnsAsync(existingMovies.First());

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(0, result);
        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task SyncMoviesCommandHandler_WithApiError_ShouldThrowException()
    {
        // Arrange
        var handler = new SyncMoviesCommandHandler(
            _mockMovieRepository.Object, 
            _mockStarWarsApiService.Object, 
            _mockSyncLogger.Object);
        
        var command = new SyncMoviesCommand { UserId = "admin-123" };

        _mockStarWarsApiService.Setup(x => x.GetFilmsAsync())
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => handler.HandleAsync(command));

        Assert.Contains("API Error", exception.Message);
        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Never);
    }

    #endregion
} 