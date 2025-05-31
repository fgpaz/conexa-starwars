using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;

namespace ConexaStarWars.Core.Commands;

public class CreateMovieCommand : IRequest<MovieDto>
{
    public CreateMovieDto MovieData { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

public class UpdateMovieCommand : IRequest<MovieDto?>
{
    public int MovieId { get; set; }
    public UpdateMovieDto MovieData { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

public class DeleteMovieCommand : IRequest<bool>
{
    public int MovieId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class SyncMoviesCommand : IRequest<int>
{
    public string UserId { get; set; } = string.Empty;
}