using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Commands;

public class SyncMoviesCommandHandler(
    IRepository<Movie> movieRepository,
    IStarWarsApiService starWarsApiService,
    ILogger<SyncMoviesCommandHandler> logger)
    : IRequestHandler<SyncMoviesCommand, int>
{
    public async Task<int> HandleAsync(SyncMoviesCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando comando SyncMovies por usuario {UserId}", request.UserId);

        try
        {
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
                    logger.LogInformation("Película sincronizada: {MovieTitle}", film.Title);
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
                    logger.LogInformation("Película actualizada: {MovieTitle}", film.Title);
                }
            }

            logger.LogInformation("Sincronización completada por usuario {UserId}. {SyncedCount} películas nuevas sincronizadas",
                request.UserId, syncedCount);

            return syncedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la sincronización por usuario {UserId}", request.UserId);
            throw;
        }
    }
}