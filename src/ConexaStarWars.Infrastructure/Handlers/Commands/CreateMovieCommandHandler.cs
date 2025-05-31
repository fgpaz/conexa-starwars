using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Commands;

public class CreateMovieCommandHandler(
    IRepository<Movie> movieRepository,
    ILogger<CreateMovieCommandHandler> logger) : IRequestHandler<CreateMovieCommand, MovieDto>
{
    public async Task<MovieDto> HandleAsync(CreateMovieCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando comando CreateMovie para usuario {UserId}", request.UserId);

        try
        {
            // Validar UserId
            if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("El ID del usuario es requerido");
            
            // Validar datos de entrada
            if (request.MovieData == null) throw new ArgumentNullException(nameof(request.MovieData), "Los datos de la película son requeridos");

            if (string.IsNullOrWhiteSpace(request.MovieData.Title)) throw new ArgumentException("El título de la película es requerido");

            if (request.MovieData.EpisodeId <= 0) throw new ArgumentException("El Episode ID debe ser mayor a 0");

            // Verificar si ya existe una película con el mismo EpisodeId
            var existingMovie = await movieRepository.GetAsync(m => m.EpisodeId == request.MovieData.EpisodeId);
            if (existingMovie != null) throw new InvalidOperationException($"Ya existe una película con el Episode ID {request.MovieData.EpisodeId}");

            var movie = MovieMapper.ToEntity(request.MovieData);
            movie.CreatedAt = DateTime.UtcNow;

            var createdMovie = await movieRepository.AddAsync(movie);
            var result = MovieMapper.ToDto(createdMovie);

            logger.LogInformation("Película creada exitosamente con ID {MovieId} por usuario {UserId}",
                result.Id, request.UserId);

            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            logger.LogError(ex, "Error inesperado al crear película para usuario {UserId}", request.UserId);
            throw new InvalidOperationException("Error interno al crear la película", ex);
        }
    }
}