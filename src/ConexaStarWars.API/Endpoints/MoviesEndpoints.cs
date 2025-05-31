using System.Security.Claims;
using ConexaStarWars.API.Filters;
using ConexaStarWars.API.Middleware;
using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.Constants;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConexaStarWars.API.Endpoints;

public static class MoviesEndpoints
{
    public static void MapMoviesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies")
            .WithTags("Movies")
            .RequireAuthorization();

        // GET /api/movies
        group.MapGet("/", GetAllMoviesAsync)
            .WithName("GetAllMovies")
            .WithSummary("Obtiene la lista de todas las películas")
            .WithDescription("Devuelve una lista paginada de películas con filtros opcionales")
            .Produces<IEnumerable<MovieDto>>()
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        // GET /api/movies/{id}
        group.MapGet("/{id:int}", GetMovieByIdAsync)
            .WithName("GetMovieById")
            .WithSummary("Obtiene los detalles de una película específica")
            .WithDescription("Devuelve los detalles completos de una película por su ID")
            .RequireAuthorization(policy => policy.RequireRole(Roles.RegularUser, Roles.Administrator))
            .Produces<MovieDto>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        // POST /api/movies
        group.MapPost("/", CreateMovieAsync)
            .WithName("CreateMovie")
            .WithSummary("Crea una nueva película (Solo Administradores)")
            .WithDescription("Crea una nueva película en el sistema")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces<MovieDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        // PUT /api/movies/{id}
        group.MapPut("/{id:int}", UpdateMovieAsync)
            .WithName("UpdateMovie")
            .WithSummary("Actualiza una película existente (Solo Administradores)")
            .WithDescription("Actualiza los datos de una película existente")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces<MovieDto>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        // DELETE /api/movies/{id}
        group.MapDelete("/{id:int}", DeleteMovieAsync)
            .WithName("DeleteMovie")
            .WithSummary("Elimina una película (Solo Administradores)")
            .WithDescription("Elimina permanentemente una película del sistema")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        // POST /api/movies/sync
        group.MapPost("/sync", SyncMoviesFromStarWarsApiAsync)
            .WithName("SyncMovies")
            .WithSummary("Sincroniza películas desde la API de Star Wars (Solo Administradores)")
            .WithDescription("Obtiene y sincroniza las películas desde la API externa de Star Wars")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces<object>()
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetAllMoviesAsync(
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var query = new GetAllMoviesQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var movies = await mediator.SendAsync(query);
        return Results.Ok(movies);
    }

    private static async Task<IResult> GetMovieByIdAsync(
        int id,
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var query = new GetMovieByIdQuery
        {
            MovieId = id,
            UserId = userId
        };

        var movie = await mediator.SendAsync(query);
        return Results.Ok(movie);
    }

    private static async Task<IResult> CreateMovieAsync(
        [FromBody] CreateMovieDto createMovieDto,
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger)
    {
        // Validar el modelo
        ValidationFilter.ValidateModel(createMovieDto);

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var command = new CreateMovieCommand
        {
            MovieData = createMovieDto,
            UserId = userId
        };

        var movie = await mediator.SendAsync(command);
        logger.LogInformation("Película creada exitosamente: {Title}", movie.Title);

        return Results.Created($"/api/movies/{movie.Id}", movie);
    }

    private static async Task<IResult> UpdateMovieAsync(
        int id,
        [FromBody] UpdateMovieDto updateMovieDto,
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger)
    {
        // Validar el modelo
        ValidationFilter.ValidateModel(updateMovieDto);

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var command = new UpdateMovieCommand
        {
            MovieId = id,
            MovieData = updateMovieDto,
            UserId = userId
        };

        var movie = await mediator.SendAsync(command);
        logger.LogInformation("Película actualizada exitosamente: {Title}", movie.Title);
        return Results.Ok(movie);
    }

    private static async Task<IResult> DeleteMovieAsync(
        int id,
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var command = new DeleteMovieCommand
        {
            MovieId = id,
            UserId = userId
        };

        var deleted = await mediator.SendAsync(command);
        if (!deleted) throw new NotFoundException($"No se encontró la película con ID {id}");

        logger.LogInformation("Película eliminada exitosamente con ID {MovieId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> SyncMoviesFromStarWarsApiAsync(
        HttpContext context,
        IMediator mediator,
        ILogger<Program> logger)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var command = new SyncMoviesCommand
        {
            UserId = userId
        };

        var syncedCount = await mediator.SendAsync(command);

        logger.LogInformation("Sincronización completada. {Count} películas procesadas", syncedCount);

        return Results.Ok(new
        {
            message = "Sincronización completada exitosamente",
            moviesProcessed = syncedCount
        });
    }
}