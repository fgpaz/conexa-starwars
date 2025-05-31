using ConexaStarWars.Core.Entities;

namespace ConexaStarWars.Core.Interfaces;

public interface IMovieSyncStrategy
{
    string StrategyName { get; }
    Task<List<Movie>> SyncMoviesAsync();
}

public interface IMovieValidationStrategy
{
    string ValidationName { get; }
    Task<bool> ValidateMovieAsync(Movie movie);
}

public interface INotificationStrategy
{
    string NotificationType { get; }
    Task SendNotificationAsync(string message, string userId);
}