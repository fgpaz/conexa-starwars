using ConexaStarWars.Core.DTOs;

namespace ConexaStarWars.Core.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto?> GetMovieByIdAsync(int id);
    Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto);
    Task<MovieDto?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto);
    Task<bool> DeleteMovieAsync(int id);
    Task<int> SyncMoviesFromStarWarsApiAsync();
}