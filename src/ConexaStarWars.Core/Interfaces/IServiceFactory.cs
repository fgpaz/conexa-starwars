namespace ConexaStarWars.Core.Interfaces;

public interface IServiceFactory
{
    T CreateService<T>() where T : class;
    IMovieService CreateMovieService();
    IAuthService CreateAuthService();
    IStarWarsApiService CreateStarWarsApiService();
}

public interface IExternalApiServiceFactory
{
    IStarWarsApiService CreateStarWarsApiService();
    // Futuras APIs externas
    // IMarvelApiService CreateMarvelApiService();
    // IDCApiService CreateDCApiService();
}