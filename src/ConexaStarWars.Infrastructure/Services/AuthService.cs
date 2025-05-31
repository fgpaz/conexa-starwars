using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ConexaStarWars.Infrastructure.Services;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el usuario ya existe
        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null) throw new InvalidOperationException("El usuario ya existe con este email");

        // Crear nuevo usuario
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al crear usuario: {errors}");
        }

        // Asignar rol de usuario regular por defecto
        await userManager.AddToRoleAsync(user, "RegularUser");

        // Generar token
        var token = await GenerateJwtTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) throw new UnauthorizedAccessException("Credenciales inválidas");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid) throw new UnauthorizedAccessException("Credenciales inválidas");

        var token = await GenerateJwtTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return false;

        var role = new IdentityRole(roleName);
        var result = await roleManager.CreateAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleToUserAsync(string email, string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        if (!await roleManager.RoleExistsAsync(roleName))
            return false;

        var result = await userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        // Agregar roles como claims
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}