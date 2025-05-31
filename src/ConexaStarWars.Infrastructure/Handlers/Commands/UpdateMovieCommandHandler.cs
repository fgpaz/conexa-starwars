using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Commands;

public class UpdateMovieCommandHandler(
    IRepository<Movie> movieRepository,
    ILogger<UpdateMovieCommandHandler> logger) : IRequestHandler<UpdateMovieCommand, MovieDto?>
{
    public async Task<MovieDto?> HandleAsync(UpdateMovieCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando comando UpdateMovie para película {MovieId} por usuario {UserId}",
            request.MovieId, request.UserId);

        var existingMovie = await movieRepository.GetByIdAsync(request.MovieId);
        if (existingMovie == null)
        {
            logger.LogWarning("Película no encontrada con ID {MovieId}", request.MovieId);
            return null;
        }

        // Verificar si el nuevo EpisodeId ya existe en otra película
        var movieWithSameEpisodeId = await movieRepository.GetAsync(m =>
            m.EpisodeId == request.MovieData.EpisodeId && m.Id != request.MovieId);

        if (movieWithSameEpisodeId != null) throw new InvalidOperationException($"Ya existe otra película con el Episode ID {request.MovieData.EpisodeId}");

        MovieMapper.UpdateEntity(existingMovie, request.MovieData);
        existingMovie.UpdatedAt = DateTime.UtcNow;

        var updatedMovie = await movieRepository.UpdateAsync(existingMovie);
        var result = MovieMapper.ToDto(updatedMovie);

        logger.LogInformation("Película actualizada exitosamente: {MovieTitle} por usuario {UserId}",
            result.Title, request.UserId);

        return result;
    }
}