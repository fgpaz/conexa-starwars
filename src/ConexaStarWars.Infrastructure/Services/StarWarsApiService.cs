using System.Text.Json;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;

namespace ConexaStarWars.Infrastructure.Services;

public class StarWarsApiService : IStarWarsApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public StarWarsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.swapi.tech/api/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<StarWarsApiResponse> GetFilmsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("films");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<StarWarsApiResponse>(jsonContent, _jsonOptions);

            return apiResponse ?? new StarWarsApiResponse();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Error al consumir la API de Star Wars: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Error al deserializar la respuesta de la API: {ex.Message}", ex);
        }
    }
}