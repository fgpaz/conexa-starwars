using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Exceptions;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using ConexaStarWars.Infrastructure.Handlers.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConexaStarWars.Tests.Handlers;

public class QueryHandlersTests
{
    private readonly Mock<IRepository<Movie>> _mockMovieRepository;
    private readonly Mock<ILogger<GetAllMoviesQueryHandler>> _mockGetAllLogger;
    private readonly Mock<ILogger<GetMovieByIdQueryHandler>> _mockGetByIdLogger;

    public QueryHandlersTests()
    {
        _mockMovieRepository = new Mock<IRepository<Movie>>();
        _mockGetAllLogger = new Mock<ILogger<GetAllMoviesQueryHandler>>();
        _mockGetByIdLogger = new Mock<ILogger<GetMovieByIdQueryHandler>>();
    }

    #region GetAllMoviesQueryHandler Tests

    [Fact]
    public async Task GetAllMoviesQueryHandler_WithoutFilters_ShouldReturnAllMovies()
    {
        // Arrange
        var handler = new GetAllMoviesQueryHandler(_mockMovieRepository.Object, _mockGetAllLogger.Object);
        
        var query = new GetAllMoviesQuery
        {
            UserId = "user-123",
            PageNumber = 1,
            PageSize = 10
        };

        var movies = new List<Movie>
        {
            new()
            {
                Id = 1,
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
                Species = new List<string> { "Human", "Droid" },
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 2,
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = new DateTime(1980, 5, 17),
                Characters = new List<string> { "Luke Skywalker", "Darth Vader" },
                Planets = new List<string> { "Hoth", "Dagobah" },
                Starships = new List<string> { "Star Destroyer", "Millennium Falcon" },
                Vehicles = new List<string> { "AT-AT", "Snowspeeder" },
                Species = new List<string> { "Human", "Yoda's species" },
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(movies);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var resultList = result.ToList();
        Assert.Equal("A New Hope", resultList[0].Title);
        Assert.Equal("The Empire Strikes Back", resultList[1].Title);
    }

    [Fact]
    public async Task GetAllMoviesQueryHandler_WithSearchTerm_ShouldReturnFilteredMovies()
    {
        // Arrange
        var handler = new GetAllMoviesQueryHandler(_mockMovieRepository.Object, _mockGetAllLogger.Object);
        
        var query = new GetAllMoviesQuery
        {
            UserId = "user-123",
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "Empire"
        };

        var movies = new List<Movie>
        {
            new()
            {
                Id = 1,
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 2,
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = new DateTime(1980, 5, 17),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(movies);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("The Empire Strikes Back", result.First().Title);
    }

    [Fact]
    public async Task GetAllMoviesQueryHandler_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var handler = new GetAllMoviesQueryHandler(_mockMovieRepository.Object, _mockGetAllLogger.Object);
        
        var query = new GetAllMoviesQuery
        {
            UserId = "user-123",
            PageNumber = 2,
            PageSize = 1
        };

        var movies = new List<Movie>
        {
            new()
            {
                Id = 1,
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = 2,
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = new DateTime(1980, 5, 17),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(movies);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("The Empire Strikes Back", result.First().Title);
    }

    [Fact]
    public async Task GetAllMoviesQueryHandler_WithEmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new GetAllMoviesQueryHandler(_mockMovieRepository.Object, _mockGetAllLogger.Object);
        
        var query = new GetAllMoviesQuery
        {
            UserId = "user-123",
            PageNumber = 1,
            PageSize = 10
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Movie>());

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("hope", "A New Hope")]
    [InlineData("EMPIRE", "The Empire Strikes Back")]
    [InlineData("lucas", "A New Hope")]
    [InlineData("kershner", "The Empire Strikes Back")]
    public async Task GetAllMoviesQueryHandler_WithDifferentSearchTerms_ShouldReturnCorrectResults(
        string searchTerm, string expectedTitle)
    {
        // Arrange
        var handler = new GetAllMoviesQueryHandler(_mockMovieRepository.Object, _mockGetAllLogger.Object);
        
        var query = new GetAllMoviesQuery
        {
            UserId = "user-123",
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        var movies = new List<Movie>
        {
            new()
            {
                Id = 1,
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 2,
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = new DateTime(1980, 5, 17),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(movies);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedTitle, result.First().Title);
    }

    #endregion

    #region GetMovieByIdQueryHandler Tests

    [Fact]
    public async Task GetMovieByIdQueryHandler_WithValidId_ShouldReturnMovie()
    {
        // Arrange
        var handler = new GetMovieByIdQueryHandler(_mockMovieRepository.Object, _mockGetByIdLogger.Object);
        
        var query = new GetMovieByIdQuery
        {
            MovieId = 1,
            UserId = "user-123"
        };

        var movie = new Movie
        {
            Id = 1,
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
            Species = new List<string> { "Human", "Droid" },
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(query.MovieId))
            .ReturnsAsync(movie);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movie.Id, result.Id);
        Assert.Equal(movie.Title, result.Title);
        Assert.Equal(movie.EpisodeId, result.EpisodeId);
        Assert.Equal(movie.Director, result.Director);
        Assert.Equal(movie.Producer, result.Producer);
        Assert.Equal(movie.OpeningCrawl, result.OpeningCrawl);
        Assert.Equal(movie.ReleaseDate, result.ReleaseDate);
        Assert.Equal(movie.Characters, result.Characters);
        Assert.Equal(movie.Planets, result.Planets);
        Assert.Equal(movie.Starships, result.Starships);
        Assert.Equal(movie.Vehicles, result.Vehicles);
        Assert.Equal(movie.Species, result.Species);
    }

    [Fact]
    public async Task GetMovieByIdQueryHandler_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetMovieByIdQueryHandler(_mockMovieRepository.Object, _mockGetByIdLogger.Object);
        
        var query = new GetMovieByIdQuery
        {
            MovieId = 999,
            UserId = "user-123"
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(query.MovieId))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConexaStarWars.Core.Exceptions.NotFoundException>(
            () => handler.HandleAsync(query));

        Assert.Contains("No se encontró la película con ID 999", exception.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public async Task GetMovieByIdQueryHandler_WithDifferentValidIds_ShouldReturnCorrectMovie(int movieId)
    {
        // Arrange
        var handler = new GetMovieByIdQueryHandler(_mockMovieRepository.Object, _mockGetByIdLogger.Object);
        
        var query = new GetMovieByIdQuery
        {
            MovieId = movieId,
            UserId = "user-123"
        };

        var movie = new Movie
        {
            Id = movieId,
            Title = $"Movie {movieId}",
            EpisodeId = movieId,
            Director = $"Director {movieId}",
            Producer = $"Producer {movieId}",
            OpeningCrawl = $"Opening crawl for movie {movieId}",
            ReleaseDate = new DateTime(1977 + movieId, 5, 25),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(query.MovieId))
            .ReturnsAsync(movie);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movieId, result.Id);
        Assert.Equal($"Movie {movieId}", result.Title);
        Assert.Equal(movieId, result.EpisodeId);
    }

    [Fact]
    public async Task GetMovieByIdQueryHandler_WithCompleteMovieData_ShouldMapAllProperties()
    {
        // Arrange
        var handler = new GetMovieByIdQueryHandler(_mockMovieRepository.Object, _mockGetByIdLogger.Object);
        
        var query = new GetMovieByIdQuery
        {
            MovieId = 1,
            UserId = "user-123"
        };

        var movie = new Movie
        {
            Id = 1,
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "It is a period of civil war. Rebel spaceships, striking from a hidden base, have won their first victory against the evil Galactic Empire.",
            ReleaseDate = new DateTime(1977, 5, 25),
            Characters = new List<string> { "Luke Skywalker", "Princess Leia", "Han Solo", "Darth Vader", "Obi-Wan Kenobi" },
            Planets = new List<string> { "Tatooine", "Alderaan", "Yavin IV" },
            Starships = new List<string> { "Death Star", "Millennium Falcon", "X-wing", "TIE Fighter" },
            Vehicles = new List<string> { "Sandcrawler", "Landspeeder" },
            Species = new List<string> { "Human", "Droid", "Wookiee", "Rodian" },
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(query.MovieId))
            .ReturnsAsync(movie);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Characters.Count);
        Assert.Equal(3, result.Planets.Count);
        Assert.Equal(4, result.Starships.Count);
        Assert.Equal(2, result.Vehicles.Count);
        Assert.Equal(4, result.Species.Count);
        Assert.Contains("Luke Skywalker", result.Characters);
        Assert.Contains("Tatooine", result.Planets);
        Assert.Contains("Death Star", result.Starships);
        Assert.Contains("Sandcrawler", result.Vehicles);
        Assert.Contains("Human", result.Species);
    }

    #endregion
} 