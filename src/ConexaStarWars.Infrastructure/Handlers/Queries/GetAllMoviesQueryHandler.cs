using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using ConexaStarWars.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Handlers.Queries;

public class GetAllMoviesQueryHandler(
    IRepository<Movie> movieRepository,
    ILogger<GetAllMoviesQueryHandler> logger) : IRequestHandler<GetAllMoviesQuery, IEnumerable<MovieDto>>
{
    public async Task<IEnumerable<MovieDto>> HandleAsync(GetAllMoviesQuery request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Procesando query GetAllMovies para usuario {UserId}", request.UserId);

        var movies = await movieRepository.GetAllAsync();

        // Aplicar filtros si es necesario
        if (!string.IsNullOrEmpty(request.SearchTerm))
            movies = movies.Where(m =>
                m.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Director.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));

        // Aplicar paginación
        var pagedMovies = movies
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var result = MovieMapper.ToDtoList(pagedMovies);

        logger.LogInformation("Query GetAllMovies procesada. Películas encontradas: {Count} para usuario {UserId}",
            result.Count(), request.UserId);

        return result;
    }
}