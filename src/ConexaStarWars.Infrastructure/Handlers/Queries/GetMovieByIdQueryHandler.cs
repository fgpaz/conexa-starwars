using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Exceptions;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using ConexaStarWars.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Queries;

public class GetMovieByIdQueryHandler(
    IRepository<Movie> movieRepository,
    ILogger<GetMovieByIdQueryHandler> logger) : IRequestHandler<GetMovieByIdQuery, MovieDto>
{
    public async Task<MovieDto> HandleAsync(GetMovieByIdQuery request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando query GetMovieById para película {MovieId} por usuario {UserId}",
            request.MovieId, request.UserId);

        try
        {
            if (request.MovieId <= 0) throw new ArgumentException("El ID de la película debe ser mayor a 0");

            var movie = await movieRepository.GetByIdAsync(request.MovieId);

            if (movie == null)
            {
                logger.LogWarning("Película no encontrada con ID {MovieId} para usuario {UserId}",
                    request.MovieId, request.UserId);
                throw new NotFoundException($"No se encontró la película con ID {request.MovieId}");
            }

            var result = MovieMapper.ToDto(movie);

            logger.LogInformation("Query GetMovieById procesada. Película encontrada: {MovieTitle} para usuario {UserId}",
                result.Title, request.UserId);

            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is NotFoundException))
        {
            logger.LogError(ex, "Error inesperado al obtener película {MovieId} para usuario {UserId}",
                request.MovieId, request.UserId);
            throw new InvalidOperationException("Error interno al obtener la película", ex);
        }
    }
}