using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Exceptions;
using ConexaStarWars.Infrastructure.Mappings;
using Xunit;

namespace ConexaStarWars.Tests.Coverage;

/// <summary>
/// Tests adicionales para asegurar 100% de cobertura de código
/// </summary>
public class AdditionalCoverageTests
{
    [Fact]
    public void NotFoundException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Test not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ValidationException_WithErrors_ShouldSetErrors()
    {
        // Arrange
        var errors = new { Field = "Error message" };

        // Act
        var exception = new ConexaStarWars.API.Middleware.ValidationException(errors);

        // Assert
        Assert.Equal(errors, exception.Errors);
        Assert.Equal("Error de validación", exception.Message);
    }

    [Fact]
    public void ForbiddenException_WithoutMessage_ShouldUseDefault()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        Assert.Equal("Acceso prohibido", exception.Message);
    }

    [Fact]
    public void ForbiddenException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Custom forbidden message";

        // Act
        var exception = new ForbiddenException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Movie_Properties_ShouldBeSettable()
    {
        // Arrange
        var movie = new Movie();
        var now = DateTime.UtcNow;

        // Act
        movie.Id = 1;
        movie.Title = "Test Movie";
        movie.EpisodeId = 4;
        movie.Director = "Test Director";
        movie.Producer = "Test Producer";
        movie.OpeningCrawl = "Test opening crawl";
        movie.ReleaseDate = now;
        movie.Characters = new List<string> { "Character1" };
        movie.Planets = new List<string> { "Planet1" };
        movie.Starships = new List<string> { "Starship1" };
        movie.Vehicles = new List<string> { "Vehicle1" };
        movie.Species = new List<string> { "Species1" };
        movie.StarWarsApiUrl = "https://test.com";
        movie.CreatedAt = now;
        movie.UpdatedAt = now;

        // Assert
        Assert.Equal(1, movie.Id);
        Assert.Equal("Test Movie", movie.Title);
        Assert.Equal(4, movie.EpisodeId);
        Assert.Equal("Test Director", movie.Director);
        Assert.Equal("Test Producer", movie.Producer);
        Assert.Equal("Test opening crawl", movie.OpeningCrawl);
        Assert.Equal(now, movie.ReleaseDate);
        Assert.Single(movie.Characters);
        Assert.Single(movie.Planets);
        Assert.Single(movie.Starships);
        Assert.Single(movie.Vehicles);
        Assert.Single(movie.Species);
        Assert.Equal("https://test.com", movie.StarWarsApiUrl);
        Assert.Equal(now, movie.CreatedAt);
        Assert.Equal(now, movie.UpdatedAt);
    }

    [Fact]
    public void CreateMovieDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new CreateMovieDto();
        var now = DateTime.UtcNow;

        // Act
        dto.Title = "Test Movie";
        dto.EpisodeId = 4;
        dto.Director = "Test Director";
        dto.Producer = "Test Producer";
        dto.OpeningCrawl = "Test opening crawl";
        dto.ReleaseDate = now;
        dto.Characters = new List<string> { "Character1" };
        dto.Planets = new List<string> { "Planet1" };
        dto.Starships = new List<string> { "Starship1" };
        dto.Vehicles = new List<string> { "Vehicle1" };
        dto.Species = new List<string> { "Species1" };

        // Assert
        Assert.Equal("Test Movie", dto.Title);
        Assert.Equal(4, dto.EpisodeId);
        Assert.Equal("Test Director", dto.Director);
        Assert.Equal("Test Producer", dto.Producer);
        Assert.Equal("Test opening crawl", dto.OpeningCrawl);
        Assert.Equal(now, dto.ReleaseDate);
        Assert.Single(dto.Characters);
        Assert.Single(dto.Planets);
        Assert.Single(dto.Starships);
        Assert.Single(dto.Vehicles);
        Assert.Single(dto.Species);
    }

    [Fact]
    public void UpdateMovieDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new UpdateMovieDto();
        var now = DateTime.UtcNow;

        // Act
        dto.Title = "Test Movie";
        dto.EpisodeId = 4;
        dto.Director = "Test Director";
        dto.Producer = "Test Producer";
        dto.OpeningCrawl = "Test opening crawl";
        dto.ReleaseDate = now;
        dto.Characters = new List<string> { "Character1" };
        dto.Planets = new List<string> { "Planet1" };
        dto.Starships = new List<string> { "Starship1" };
        dto.Vehicles = new List<string> { "Vehicle1" };
        dto.Species = new List<string> { "Species1" };

        // Assert
        Assert.Equal("Test Movie", dto.Title);
        Assert.Equal(4, dto.EpisodeId);
        Assert.Equal("Test Director", dto.Director);
        Assert.Equal("Test Producer", dto.Producer);
        Assert.Equal("Test opening crawl", dto.OpeningCrawl);
        Assert.Equal(now, dto.ReleaseDate);
        Assert.Single(dto.Characters);
        Assert.Single(dto.Planets);
        Assert.Single(dto.Starships);
        Assert.Single(dto.Vehicles);
        Assert.Single(dto.Species);
    }

    [Fact]
    public void MovieDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new MovieDto();
        var now = DateTime.UtcNow;

        // Act
        dto.Id = 1;
        dto.Title = "Test Movie";
        dto.EpisodeId = 4;
        dto.Director = "Test Director";
        dto.Producer = "Test Producer";
        dto.OpeningCrawl = "Test opening crawl";
        dto.ReleaseDate = now;
        dto.Characters = new List<string> { "Character1" };
        dto.Planets = new List<string> { "Planet1" };
        dto.Starships = new List<string> { "Starship1" };
        dto.Vehicles = new List<string> { "Vehicle1" };
        dto.Species = new List<string> { "Species1" };
        dto.CreatedAt = now;
        dto.UpdatedAt = now;

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Test Movie", dto.Title);
        Assert.Equal(4, dto.EpisodeId);
        Assert.Equal("Test Director", dto.Director);
        Assert.Equal("Test Producer", dto.Producer);
        Assert.Equal("Test opening crawl", dto.OpeningCrawl);
        Assert.Equal(now, dto.ReleaseDate);
        Assert.Single(dto.Characters);
        Assert.Single(dto.Planets);
        Assert.Single(dto.Starships);
        Assert.Single(dto.Vehicles);
        Assert.Single(dto.Species);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
    }

    [Fact]
    public void RegisterDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new RegisterDto();

        // Act
        dto.Email = "test@test.com";
        dto.Password = "Password123!";
        dto.ConfirmPassword = "Password123!";
        dto.FirstName = "Test";
        dto.LastName = "User";

        // Assert
        Assert.Equal("test@test.com", dto.Email);
        Assert.Equal("Password123!", dto.Password);
        Assert.Equal("Password123!", dto.ConfirmPassword);
        Assert.Equal("Test", dto.FirstName);
        Assert.Equal("User", dto.LastName);
    }

    [Fact]
    public void LoginDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new LoginDto();

        // Act
        dto.Email = "test@test.com";
        dto.Password = "Password123!";

        // Assert
        Assert.Equal("test@test.com", dto.Email);
        Assert.Equal("Password123!", dto.Password);
    }

    [Fact]
    public void AuthResponseDto_Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new AuthResponseDto();
        var now = DateTime.UtcNow;

        // Act
        dto.Token = "token";
        dto.Email = "test@test.com";
        dto.FirstName = "Test";
        dto.LastName = "User";
        dto.Roles = new List<string> { "Admin" };
        dto.ExpiresAt = now;

        // Assert
        Assert.Equal("token", dto.Token);
        Assert.Equal("test@test.com", dto.Email);
        Assert.Equal("Test", dto.FirstName);
        Assert.Equal("User", dto.LastName);
        Assert.Single(dto.Roles);
        Assert.Equal(now, dto.ExpiresAt);
    }

    [Fact]
    public void StarWarsApiResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new StarWarsApiResponse();
        var results = new List<StarWarsFilmResult>();

        // Act
        response.Results = results;

        // Assert
        Assert.Equal(results, response.Results);
    }

    [Fact]
    public void StarWarsFilmResult_Properties_ShouldBeSettable()
    {
        // Arrange
        var result = new StarWarsFilmResult();
        var properties = new StarWarsFilmProperties();

        // Act
        result.Properties = properties;

        // Assert
        Assert.Equal(properties, result.Properties);
    }

    [Fact]
    public void StarWarsFilmProperties_Properties_ShouldBeSettable()
    {
        // Arrange
        var properties = new StarWarsFilmProperties();

        // Act
        properties.Title = "Test";
        properties.Episode_id = 1;
        properties.Opening_crawl = "Test";
        properties.Director = "Test";
        properties.Producer = "Test";
        properties.Release_date = "2023-01-01";
        properties.Characters = new List<string> { "Test" };
        properties.Planets = new List<string> { "Test" };
        properties.Starships = new List<string> { "Test" };
        properties.Vehicles = new List<string> { "Test" };
        properties.Species = new List<string> { "Test" };
        properties.Url = "https://test.com";

        // Assert
        Assert.Equal("Test", properties.Title);
        Assert.Equal(1, properties.Episode_id);
        Assert.Equal("Test", properties.Opening_crawl);
        Assert.Equal("Test", properties.Director);
        Assert.Equal("Test", properties.Producer);
        Assert.Equal("2023-01-01", properties.Release_date);
        Assert.Single(properties.Characters);
        Assert.Single(properties.Planets);
        Assert.Single(properties.Starships);
        Assert.Single(properties.Vehicles);
        Assert.Single(properties.Species);
        Assert.Equal("https://test.com", properties.Url);
    }

    [Fact]
    public void MovieMapper_ToDto_ShouldMapCorrectly()
    {
        // Arrange
        var movie = new Movie
        {
            Id = 1,
            Title = "Test Movie",
            EpisodeId = 4,
            Director = "Test Director",
            Producer = "Test Producer",
            OpeningCrawl = "Test opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string> { "Character1" },
            Planets = new List<string> { "Planet1" },
            Starships = new List<string> { "Starship1" },
            Vehicles = new List<string> { "Vehicle1" },
            Species = new List<string> { "Species1" },
            StarWarsApiUrl = "https://test.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = MovieMapper.ToDto(movie);

        // Assert
        Assert.Equal(movie.Id, dto.Id);
        Assert.Equal(movie.Title, dto.Title);
        Assert.Equal(movie.EpisodeId, dto.EpisodeId);
        Assert.Equal(movie.Director, dto.Director);
        Assert.Equal(movie.Producer, dto.Producer);
    }

    [Fact]
    public void MovieMapper_ToEntity_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Test Movie",
            EpisodeId = 4,
            Director = "Test Director",
            Producer = "Test Producer",
            OpeningCrawl = "Test opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string> { "Character1" },
            Planets = new List<string> { "Planet1" },
            Starships = new List<string> { "Starship1" },
            Vehicles = new List<string> { "Vehicle1" },
            Species = new List<string> { "Species1" }
        };

        // Act
        var entity = MovieMapper.ToEntity(dto);

        // Assert
        Assert.Equal(dto.Title, entity.Title);
        Assert.Equal(dto.EpisodeId, entity.EpisodeId);
        Assert.Equal(dto.Director, entity.Director);
        Assert.Equal(dto.Producer, entity.Producer);
    }

    [Fact]
    public void MovieMapper_UpdateEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var entity = new Movie
        {
            Id = 1,
            Title = "Old Title",
            EpisodeId = 1,
            Director = "Old Director"
        };

        var dto = new UpdateMovieDto
        {
            Title = "New Title",
            EpisodeId = 2,
            Director = "New Director",
            Producer = "New Producer",
            OpeningCrawl = "New opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string> { "Character1" },
            Planets = new List<string> { "Planet1" },
            Starships = new List<string> { "Starship1" },
            Vehicles = new List<string> { "Vehicle1" },
            Species = new List<string> { "Species1" }
        };

        // Act
        MovieMapper.UpdateEntity(entity, dto);

        // Assert
        Assert.Equal(dto.Title, entity.Title);
        Assert.Equal(dto.EpisodeId, entity.EpisodeId);
        Assert.Equal(dto.Director, entity.Director);
        Assert.Equal(dto.Producer, entity.Producer);
        Assert.Equal(1, entity.Id); // ID should not change
    }
} 