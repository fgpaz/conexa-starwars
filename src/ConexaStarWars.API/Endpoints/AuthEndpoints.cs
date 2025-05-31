using ConexaStarWars.API.Filters;
using ConexaStarWars.API.Middleware;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConexaStarWars.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/register
        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Registra un nuevo usuario en el sistema")
            .WithDescription("Crea una nueva cuenta de usuario y devuelve un token de autenticación")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        // POST /api/auth/login
        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Autentica un usuario y devuelve un token JWT")
            .WithDescription("Valida las credenciales del usuario y genera un token de acceso")
            .Produces<AuthResponseDto>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterDto registerDto,
        IAuthService authService,
        ILogger<Program> logger)
    {
        // Validar el modelo
        ValidationFilter.ValidateModel(registerDto);

        var result = await authService.RegisterAsync(registerDto);
        logger.LogInformation("Usuario registrado exitosamente: {Email}", registerDto.Email);

        return Results.Created("/api/auth/register", result);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginDto loginDto,
        IAuthService authService,
        ILogger<Program> logger)
    {
        // Validar el modelo
        ValidationFilter.ValidateModel(loginDto);

        var result = await authService.LoginAsync(loginDto);
        logger.LogInformation("Usuario autenticado exitosamente: {Email}", loginDto.Email);

        return Results.Ok(result);
    }
}