using System.Net;
using System.Text.Json;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace ConexaStarWars.Tests.Services;

public class StarWarsApiServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<StarWarsApiService>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly StarWarsApiService _starWarsApiService;

    public StarWarsApiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<StarWarsApiService>>();
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://swapi.tech/api/")
        };
        
        _starWarsApiService = new StarWarsApiService(_httpClient);
    }

    [Fact]
    public async Task GetFilmsAsync_WithValidResponse_ShouldReturnFilms()
    {
        // Arrange
        var mockResponse = new StarWarsApiResponse
        {
            Results = new List<StarWarsFilmResult>
            {
                new()
                {
                    Properties = new StarWarsFilmProperties
                    {
                        Title = "A New Hope",
                        Episode_id = 4,
                        Opening_crawl = "It is a period of civil war...",
                        Director = "George Lucas",
                        Producer = "Gary Kurtz, Rick McCallum",
                        Release_date = "1977-05-25",
                        Characters = new List<string> { "Luke Skywalker" },
                        Planets = new List<string> { "Tatooine" },
                        Starships = new List<string> { "Death Star" },
                        Vehicles = new List<string> { "Sandcrawler" },
                        Species = new List<string> { "Human" }
                    }
                },
                new()
                {
                    Properties = new StarWarsFilmProperties
                    {
                        Title = "The Empire Strikes Back",
                        Episode_id = 5,
                        Opening_crawl = "It is a dark time for the Rebellion...",
                        Director = "Irvin Kershner",
                        Producer = "Gary Kurtz, Rick McCallum",
                        Release_date = "1980-05-17",
                        Characters = new List<string> { "Luke Skywalker" },
                        Planets = new List<string> { "Hoth" },
                        Starships = new List<string> { "Star Destroyer" },
                        Vehicles = new List<string> { "AT-AT" },
                        Species = new List<string> { "Human" }
                    }
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _starWarsApiService.GetFilmsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Results.Count);
        
        var firstFilm = result.Results.First().Properties;
        Assert.Equal("A New Hope", firstFilm.Title);
        Assert.Equal(4, firstFilm.Episode_id);
        Assert.Equal("George Lucas", firstFilm.Director);
        Assert.Equal("1977-05-25", firstFilm.Release_date);
    }

    [Fact]
    public async Task GetFilmsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var mockResponse = new StarWarsApiResponse { Results = new List<StarWarsFilmResult>() };
        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _starWarsApiService.GetFilmsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task GetFilmsAsync_WithHttpError_ShouldThrowException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _starWarsApiService.GetFilmsAsync());

        Assert.Contains("Error al consumir la API de Star Wars", exception.Message);
    }

    [Fact]
    public async Task GetFilmsAsync_WithInvalidJson_ShouldThrowException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invalidJson)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _starWarsApiService.GetFilmsAsync());

        Assert.Contains("Error al deserializar la respuesta de la API", exception.Message);
    }

    [Fact]
    public async Task GetFilmsAsync_WithNetworkTimeout_ShouldThrowException()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => _starWarsApiService.GetFilmsAsync());

        Assert.Contains("Request timeout", exception.Message);
    }

    [Theory]
    [InlineData("1977-05-25", 1977, 5, 25)]
    [InlineData("1980-05-17", 1980, 5, 17)]
    [InlineData("1983-05-25", 1983, 5, 25)]
    public async Task GetFilmsAsync_WithDifferentDates_ShouldParseCorrectly(
        string dateString, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Arrange
        var mockResponse = new StarWarsApiResponse
        {
            Results = new List<StarWarsFilmResult>
            {
                new()
                {
                    Properties = new StarWarsFilmProperties
                    {
                        Title = "Test Movie",
                        Episode_id = 1,
                        Opening_crawl = "Test crawl",
                        Director = "Test Director",
                        Producer = "Test Producer",
                        Release_date = dateString,
                        Characters = new List<string>(),
                        Planets = new List<string>(),
                        Starships = new List<string>(),
                        Vehicles = new List<string>(),
                        Species = new List<string>()
                    }
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _starWarsApiService.GetFilmsAsync();

        // Assert
        var film = result.Results.First().Properties;
        Assert.Equal(expectedYear, DateTime.Parse(film.Release_date).Year);
        Assert.Equal(expectedMonth, DateTime.Parse(film.Release_date).Month);
        Assert.Equal(expectedDay, DateTime.Parse(film.Release_date).Day);
    }

    [Fact]
    public async Task GetFilmsAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var mockResponse = new StarWarsApiResponse
        {
            Results = new List<StarWarsFilmResult>
            {
                new()
                {
                    Properties = new StarWarsFilmProperties
                    {
                        Title = "A New Hope: Special Edition™",
                        Episode_id = 4,
                        Opening_crawl = "It is a period of civil war...\n\nWith special characters: áéíóú ñ",
                        Director = "George Lucas & Co.",
                        Producer = "Gary Kurtz, Rick McCallum",
                        Release_date = "1977-05-25",
                        Characters = new List<string> { "Luke Skywalker", "Princess Leia Organa" },
                        Planets = new List<string> { "Tatooine", "Alderaan" },
                        Starships = new List<string> { "Death Star", "Millennium Falcon" },
                        Vehicles = new List<string> { "Sandcrawler" },
                        Species = new List<string> { "Human", "Droid" }
                    }
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _starWarsApiService.GetFilmsAsync();

        // Assert
        var film = result.Results.First().Properties;
        Assert.Equal("A New Hope: Special Edition™", film.Title);
        Assert.Contains("áéíóú ñ", film.Opening_crawl);
        Assert.Equal("George Lucas & Co.", film.Director);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task GetFilmsAsync_WithNullResponse_ShouldThrowException()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpResponseMessage)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _starWarsApiService.GetFilmsAsync());

        // El mensaje puede variar dependiendo de la implementación interna
        Assert.True(exception.Message.Contains("Error al consumir la API de Star Wars") || 
                   exception.Message.Contains("Handler did not return a response message"),
                   $"Mensaje de error inesperado: {exception.Message}");
    }

    [Fact]
    public async Task GetFilmsAsync_WithEmptyContent_ShouldThrowException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _starWarsApiService.GetFilmsAsync());

        Assert.Contains("Error al deserializar la respuesta de la API", exception.Message);
    }

    [Fact]
    public async Task GetFilmsAsync_WithMalformedJson_ShouldThrowException()
    {
        // Arrange
        var malformedJson = "{ \"results\": [{ \"properties\": { \"title\": \"incomplete";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedJson)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _starWarsApiService.GetFilmsAsync());

        Assert.Contains("Error al deserializar la respuesta de la API", exception.Message);
    }
} 