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

        var result = await movieRepository.DeleteAsync(request.MovieId);

        if (result)
            logger.LogInformation("Película eliminada exitosamente con ID {MovieId} por usuario {UserId}",
                request.MovieId, request.UserId);
        else
            logger.LogWarning("No se pudo eliminar la película con ID {MovieId} - no encontrada", request.MovieId);

        return result;
    }
}