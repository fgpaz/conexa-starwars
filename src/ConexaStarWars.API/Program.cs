using System.Reflection;
using System.Text;
using ConexaStarWars.API.Endpoints;
using ConexaStarWars.API.Middleware;
using ConexaStarWars.Core.Commands;
using ConexaStarWars.Core.Constants;
using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Core.Queries;
using ConexaStarWars.Infrastructure.Data;
using ConexaStarWars.Infrastructure.Handlers.Commands;
using ConexaStarWars.Infrastructure.Handlers.Queries;
using ConexaStarWars.Infrastructure.Mappings;
using ConexaStarWars.Infrastructure.Repositories;
using ConexaStarWars.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/conexa-starwars-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Remover AddControllers() ya que usaremos Minimal APIs

// Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        // Usar InMemory para desarrollo
        options.UseInMemoryDatabase("ConexaStarWarsDb");
    else
        // Usar SQL Server para producción
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Autorización
builder.Services.AddAuthorization();

// HttpClient
builder.Services.AddHttpClient<IStarWarsApiService, StarWarsApiService>();

// Repository and Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IStarWarsApiService, StarWarsApiService>();

// CQRS - Mediator
builder.Services.AddScoped<IMediator, Mediator>();

// CQRS - Command Handlers
builder.Services.AddScoped<CreateMovieCommandHandler>();
builder.Services.AddScoped<UpdateMovieCommandHandler>();
builder.Services.AddScoped<DeleteMovieCommandHandler>();
builder.Services.AddScoped<SyncMoviesCommandHandler>();

// CQRS - Query Handlers
builder.Services.AddScoped<GetAllMoviesQueryHandler>();
builder.Services.AddScoped<GetMovieByIdQueryHandler>();

// Registrar handlers con sus interfaces
builder.Services.AddScoped<IRequestHandler<CreateMovieCommand, MovieDto>>(provider =>
    provider.GetRequiredService<CreateMovieCommandHandler>());

builder.Services.AddScoped<IRequestHandler<UpdateMovieCommand, MovieDto?>>(provider =>
    provider.GetRequiredService<UpdateMovieCommandHandler>());

builder.Services.AddScoped<IRequestHandler<DeleteMovieCommand, bool>>(provider =>
    provider.GetRequiredService<DeleteMovieCommandHandler>());

builder.Services.AddScoped<IRequestHandler<SyncMoviesCommand, int>>(provider =>
    provider.GetRequiredService<SyncMoviesCommandHandler>());

builder.Services.AddScoped<IRequestHandler<GetAllMoviesQuery, IEnumerable<MovieDto>>>(provider =>
    provider.GetRequiredService<GetAllMoviesQueryHandler>());

builder.Services.AddScoped<IRequestHandler<GetMovieByIdQuery, MovieDto>>(provider =>
    provider.GetRequiredService<GetMovieByIdQueryHandler>());

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conexa Star Wars API",
        Version = "v1",
        Description = "API para gestión de películas de Star Wars con autenticación JWT, arquitectura CQRS, Minimal APIs y manejo robusto de errores",
        Contact = new OpenApiContact
        {
            Name = "Conexa Star Wars API",
            Email = "contacto@conexa.com"
        }
    });

    // Configuración para JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware de manejo de errores (debe ir primero)
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conexa Star Wars API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Mapear Minimal API Endpoints
app.MapAuthEndpoints();
app.MapMoviesEndpoints();

// Inicializar datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await InitializeDataAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error durante la inicialización de datos");
    }
}

Log.Information("Aplicación iniciada exitosamente con arquitectura CQRS, Minimal APIs y manejo robusto de errores");

app.Run();

// Método para inicializar datos
static async Task InitializeDataAsync(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
{
    // Asegurar que la base de datos esté creada
    await context.Database.EnsureCreatedAsync();

    // Crear roles si no existen
    if (!await roleManager.RoleExistsAsync(Roles.Administrator)) await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));

    if (!await roleManager.RoleExistsAsync(Roles.RegularUser)) await roleManager.CreateAsync(new IdentityRole(Roles.RegularUser));

    // Crear usuario administrador por defecto
    var adminEmail = "admin@conexa.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Conexa",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
            Log.Information($"Usuario administrador creado: {adminEmail}");
        }
    }

    // Crear usuario regular por defecto
    var userEmail = "user@conexa.com";
    var regularUser = await userManager.FindByEmailAsync(userEmail);

    if (regularUser == null)
    {
        regularUser = new User
        {
            UserName = userEmail,
            Email = userEmail,
            FirstName = "Usuario",
            LastName = "Regular",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(regularUser, "User123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(regularUser, Roles.RegularUser);
            Log.Information($"Usuario regular creado: {userEmail}");
        }
    }
}

// Hacer la clase Program pública para las pruebas
public partial class Program
{
}