using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Commands;

public class DeleteMovieCommandHandler(
    IRepository<Movie> movieRepository,
    ILogger<DeleteMovieCommandHandler> logger) : IRequestHandler<DeleteMovieCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteMovieCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando comando DeleteMovie para película {MovieId} por usuario {UserId}",
            request.MovieId, request.UserId);

        try
        {
            // Validar UserId
            if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("El ID del usuario es requerido");
            
            // Validar MovieId
            if (request.MovieId <= 0) throw new ArgumentException("El ID de la película debe ser mayor a 0");

            var result = await movieRepository.DeleteAsync(request.MovieId);

            if (result)
            {
                logger.LogInformation("Película eliminada exitosamente con ID {MovieId} por usuario {UserId}",
                    request.MovieId, request.UserId);
            }
            else
            {
                logger.LogWarning("No se pudo eliminar la película con ID {MovieId} para usuario {UserId}",
                    request.MovieId, request.UserId);
            }

            return result;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            logger.LogError(ex, "Error inesperado al eliminar película {MovieId} para usuario {UserId}",
                request.MovieId, request.UserId);
            throw new InvalidOperationException("Error interno al eliminar la película", ex);
        }
    }
}