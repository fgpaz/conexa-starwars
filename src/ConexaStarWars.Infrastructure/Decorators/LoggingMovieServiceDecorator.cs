using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Decorators;

public class LoggingMovieServiceDecorator(IMovieService movieService, ILogger<LoggingMovieServiceDecorator> logger) : IMovieService
{
    public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
    {
        logger.LogInformation("Iniciando obtención de todas las películas");
        var startTime = DateTime.UtcNow;

        try
        {
            var result = await movieService.GetAllMoviesAsync();
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("Películas obtenidas exitosamente. Cantidad: {Count}, Duración: {Duration}ms",
                result.Count(), duration.TotalMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener todas las películas");
            throw;
        }
    }

    public async Task<MovieDto?> GetMovieByIdAsync(int id)
    {
        logger.LogInformation("Obteniendo película con ID: {MovieId}", id);

        try
        {
            var result = await movieService.GetMovieByIdAsync(id);
            if (result != null)
                logger.LogInformation("Película encontrada: {MovieTitle}", result.Title);
            else
                logger.LogWarning("Película no encontrada con ID: {MovieId}", id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener película con ID: {MovieId}", id);
            throw;
        }
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto)
    {
        logger.LogInformation("Creando nueva película: {MovieTitle}", createMovieDto.Title);

        try
        {
            var result = await movieService.CreateMovieAsync(createMovieDto);
            logger.LogInformation("Película creada exitosamente con ID: {MovieId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear película: {MovieTitle}", createMovieDto.Title);
            throw;
        }
    }

    public async Task<MovieDto?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto)
    {
        logger.LogInformation("Actualizando película con ID: {MovieId}", id);

        try
        {
            var result = await movieService.UpdateMovieAsync(id, updateMovieDto);
            if (result != null) logger.LogInformation("Película actualizada exitosamente: {MovieTitle}", result.Title);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar película con ID: {MovieId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        logger.LogInformation("Eliminando película con ID: {MovieId}", id);

        try
        {
            var result = await movieService.DeleteMovieAsync(id);
            if (result)
                logger.LogInformation("Película eliminada exitosamente con ID: {MovieId}", id);
            else
                logger.LogWarning("No se pudo eliminar la película con ID: {MovieId}", id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar película con ID: {MovieId}", id);
            throw;
        }
    }

    public async Task<int> SyncMoviesFromStarWarsApiAsync()
    {
        logger.LogInformation("Iniciando sincronización de películas desde Star Wars API");
        var startTime = DateTime.UtcNow;

        try
        {
            var result = await movieService.SyncMoviesFromStarWarsApiAsync();
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("Sincronización completada. Películas sincronizadas: {Count}, Duración: {Duration}ms",
                result, duration.TotalMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la sincronización de películas");
            throw;
        }
    }
}