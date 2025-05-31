using ConexaStarWars.Core.DTOs;

namespace ConexaStarWars.Core.Interfaces;

public interface IStarWarsApiService
{
    Task<StarWarsApiResponse> GetFilmsAsync();
}