using ConexaStarWars.API.Filters;
using ConexaStarWars.Core.DTOs;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ConexaStarWars.Tests.Filters;

public class ValidationFilterTests
{
    [Fact]
    public void ValidateModel_WithValidModel_ShouldNotThrow()
    {
        // Arrange
        var model = new CreateMovieDto
        {
            Title = "Test Movie",
            EpisodeId = 1,
            Director = "Test Director",
            Producer = "Test Producer",
            OpeningCrawl = "Test opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string>(),
            Planets = new List<string>(),
            Starships = new List<string>(),
            Vehicles = new List<string>(),
            Species = new List<string>()
        };

        // Act & Assert
        var result = ValidationFilter.ValidateModel(model);
        
        // No debe lanzar excepción
    }

    [Fact]
    public void ValidateModel_WithInvalidModel_ShouldThrowValidationException()
    {
        // Arrange
        var model = new CreateMovieDto
        {
            Title = "", // Inválido - requerido
            EpisodeId = 0, // Inválido - debe ser mayor a 0
            Director = "Test Director",
            Producer = "Test Producer",
            OpeningCrawl = "Test opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string>(),
            Planets = new List<string>(),
            Starships = new List<string>(),
            Vehicles = new List<string>(),
            Species = new List<string>()
        };

        // Act & Assert
        var exception = Assert.Throws<ConexaStarWars.API.Middleware.ValidationException>(
            () => ValidationFilter.ValidateModel(model));

        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public void ValidateModel_WithNullModel_ShouldThrow()
    {
        // Arrange
        CreateMovieDto? model = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => ValidationFilter.ValidateModel(model!));
    }

    [Fact]
    public void ValidateModel_WithRegisterDto_WithMismatchedPasswords_ShouldThrowValidationException()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword!", // No coincide
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var exception = Assert.Throws<ConexaStarWars.API.Middleware.ValidationException>(
            () => ValidationFilter.ValidateModel(model));

        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public void ValidateModel_WithValidRegisterDto_ShouldNotThrow()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var result = ValidationFilter.ValidateModel(model);
        
        // No debe lanzar excepción
    }

    [Fact]
    public void ValidateModel_WithInvalidEmail_ShouldThrowValidationException()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "invalid-email", // Email inválido
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var exception = Assert.Throws<ConexaStarWars.API.Middleware.ValidationException>(
            () => ValidationFilter.ValidateModel(model));

        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public void ValidateModel_WithInvalidLoginDto_ShouldThrowValidationException()
    {
        // Arrange
        var model = new LoginDto
        {
            Email = "", // Email requerido
            Password = "" // Password requerido
        };

        // Act & Assert
        var exception = Assert.Throws<ConexaStarWars.API.Middleware.ValidationException>(
            () => ValidationFilter.ValidateModel(model));

        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public void ValidateModel_WithValidLoginDto_ShouldNotThrow()
    {
        // Arrange
        var model = new LoginDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        // Act & Assert
        var result = ValidationFilter.ValidateModel(model);
        
        // No debe lanzar excepción
    }

    [Fact]
    public void ValidateModel_WithTooLongTitle_ShouldThrowValidationException()
    {
        // Arrange
        var model = new CreateMovieDto
        {
            Title = new string('A', 201), // Título demasiado largo (max 200)
            EpisodeId = 1,
            Director = "Test Director",
            Producer = "Test Producer",
            OpeningCrawl = "Test opening crawl",
            ReleaseDate = DateTime.UtcNow,
            Characters = new List<string>(),
            Planets = new List<string>(),
            Starships = new List<string>(),
            Vehicles = new List<string>(),
            Species = new List<string>()
        };

        // Act & Assert
        var exception = Assert.Throws<ConexaStarWars.API.Middleware.ValidationException>(
            () => ValidationFilter.ValidateModel(model));

        Assert.NotNull(exception.Errors);
    }
} 