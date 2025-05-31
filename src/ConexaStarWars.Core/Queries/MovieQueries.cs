using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;

namespace ConexaStarWars.Core.Queries;

public class GetAllMoviesQuery : IRequest<IEnumerable<MovieDto>>
{
    public string UserId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class GetMovieByIdQuery : IRequest<MovieDto>
{
    public int MovieId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class GetMoviesByEpisodeQuery : IRequest<IEnumerable<MovieDto>>
{
    public int EpisodeId { get; set; }
    public string UserId { get; set; } = string.Empty;
}