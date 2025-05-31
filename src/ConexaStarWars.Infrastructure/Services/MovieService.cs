using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Services;

public class MovieService(
    IRepository<Movie> movieRepository,
    IStarWarsApiService starWarsApiService,
    ILogger<MovieService> logger)
    : IMovieService
{
    public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await movieRepository.GetAllAsync();
        return MovieMapper.ToDtoList(movies);
    }

    public async Task<MovieDto?> GetMovieByIdAsync(int id)
    {
        var movie = await movieRepository.GetByIdAsync(id);
        return movie != null ? MovieMapper.ToDto(movie) : null;
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto)
    {
        // Verificar si ya existe una película con el mismo EpisodeId
        var existingMovie = await movieRepository.GetAsync(m => m.EpisodeId == createMovieDto.EpisodeId);
        if (existingMovie != null) throw new InvalidOperationException($"Ya existe una película con el Episode ID {createMovieDto.EpisodeId}");

        var movie = MovieMapper.ToEntity(createMovieDto);
        movie.CreatedAt = DateTime.UtcNow;

        var createdMovie = await movieRepository.AddAsync(movie);
        return MovieMapper.ToDto(createdMovie);
    }

    public async Task<MovieDto?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto)
    {
        var existingMovie = await movieRepository.GetByIdAsync(id);
        if (existingMovie == null) return null;

        // Verificar si el nuevo EpisodeId ya existe en otra película
        var movieWithSameEpisodeId = await movieRepository.GetAsync(m => m.EpisodeId == updateMovieDto.EpisodeId && m.Id != id);
        if (movieWithSameEpisodeId != null) throw new InvalidOperationException($"Ya existe otra película con el Episode ID {updateMovieDto.EpisodeId}");

        MovieMapper.UpdateEntity(existingMovie, updateMovieDto);
        existingMovie.UpdatedAt = DateTime.UtcNow;

        var updatedMovie = await movieRepository.UpdateAsync(existingMovie);
        return MovieMapper.ToDto(updatedMovie);
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        return await movieRepository.DeleteAsync(id);
    }

    public async Task<int> SyncMoviesFromStarWarsApiAsync()
    {
        try
        {
            logger.LogInformation("Iniciando sincronización con la API de Star Wars");

            var apiResponse = await starWarsApiService.GetFilmsAsync();
            var syncedCount = 0;

            foreach (var filmResult in apiResponse.Results)
            {
                var film = filmResult.Properties;

                // Verificar si la película ya existe
                var existingMovie = await movieRepository.GetAsync(m => m.EpisodeId == film.Episode_id);

                if (existingMovie == null)
                {
                    // Crear nueva película
                    var newMovie = new Movie
                    {
                        Title = film.Title,
                        EpisodeId = film.Episode_id,
                        OpeningCrawl = film.Opening_crawl,
                        Director = film.Director,
                        Producer = film.Producer,
                        ReleaseDate = DateTime.TryParse(film.Release_date, out var releaseDate) ? releaseDate : DateTime.MinValue,
                        Characters = film.Characters,
                        Planets = film.Planets,
                        Starships = film.Starships,
                        Vehicles = film.Vehicles,
                        Species = film.Species,
                        StarWarsApiUrl = film.Url,
                        CreatedAt = DateTime.UtcNow
                    };

                    await movieRepository.AddAsync(newMovie);
                    syncedCount++;
                    logger.LogInformation($"Película sincronizada: {film.Title}");
                }
                else
                {
                    // Actualizar película existente
                    existingMovie.Title = film.Title;
                    existingMovie.OpeningCrawl = film.Opening_crawl;
                    existingMovie.Director = film.Director;
                    existingMovie.Producer = film.Producer;
                    existingMovie.ReleaseDate = DateTime.TryParse(film.Release_date, out var releaseDate) ? releaseDate : existingMovie.ReleaseDate;
                    existingMovie.Characters = film.Characters;
                    existingMovie.Planets = film.Planets;
                    existingMovie.Starships = film.Starships;
                    existingMovie.Vehicles = film.Vehicles;
                    existingMovie.Species = film.Species;
                    existingMovie.StarWarsApiUrl = film.Url;
                    existingMovie.UpdatedAt = DateTime.UtcNow;

                    await movieRepository.UpdateAsync(existingMovie);
                    logger.LogInformation($"Película actualizada: {film.Title}");
                }
            }

            logger.LogInformation($"Sincronización completada. {syncedCount} películas nuevas sincronizadas");
            return syncedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la sincronización con la API de Star Wars");
            throw;
        }
    }
}