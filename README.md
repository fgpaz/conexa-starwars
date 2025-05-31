# Conexa Star Wars API

Una API REST completa para gestión de películas de Star Wars desarrollada en .NET 8 con autenticación JWT, autorización basada en roles y sincronización con la API pública de Star Wars.

## 🚀 Características

- **Autenticación JWT**: Sistema completo de registro y login con tokens JWT
- **Autorización por roles**: Usuarios regulares y administradores con permisos diferenciados
- **CRUD completo**: Operaciones completas para gestión de películas
- **Sincronización externa**: Integración con la API pública de Star Wars (swapi.tech)
- **Arquitectura limpia**: Implementación con Clean Architecture y patrones SOLID
- **Base de datos**: Entity Framework Core con soporte para SQL Server e InMemory
- **Documentación**: Swagger/OpenAPI integrado
- **Logging**: Serilog para logging estructurado
- **Pruebas unitarias**: Cobertura completa con xUnit y Moq

## 🏗️ Arquitectura

El proyecto sigue los principios de Clean Architecture:

```
src/
├── ConexaStarWars.API/          # Capa de presentación (Controllers, Program.cs)
├── ConexaStarWars.Core/         # Capa de dominio (Entities, DTOs, Interfaces)
└── ConexaStarWars.Infrastructure/ # Capa de infraestructura (Data, Services, Repositories)

tests/
└── ConexaStarWars.Tests/        # Pruebas unitarias
```

## 🛠️ Tecnologías Utilizadas

- **.NET 8**: Framework principal
- **ASP.NET Core**: Web API
- **Entity Framework Core**: ORM
- **Identity**: Autenticación y autorización
- **JWT Bearer**: Tokens de autenticación
- **AutoMapper**: Mapeo de objetos
- **Serilog**: Logging estructurado
- **Swagger/OpenAPI**: Documentación de API
- **xUnit**: Framework de pruebas
- **Moq**: Mocking para pruebas

## 📋 Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/sql-server) (opcional, usa InMemory por defecto en desarrollo)

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/conexa-starwars.git
cd conexa-starwars
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar la aplicación

El proyecto está configurado para usar una base de datos InMemory en desarrollo. Para usar SQL Server, modifica `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ConexaStarWarsDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. Ejecutar la aplicación

```bash
cd src/ConexaStarWars.API
dotnet run
```

La aplicación estará disponible en:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001
- **Swagger UI**: https://localhost:7001 (raíz del proyecto)

## 👥 Usuarios por Defecto

La aplicación crea automáticamente dos usuarios de prueba:

### Administrador
- **Email**: admin@conexa.com
- **Contraseña**: Admin123!
- **Rol**: Administrator
- **Permisos**: Acceso completo a todos los endpoints

### Usuario Regular
- **Email**: user@conexa.com
- **Contraseña**: User123!
- **Rol**: RegularUser
- **Permisos**: Solo lectura de películas

## 📚 Documentación de la API

### Endpoints de Autenticación

#### POST /api/auth/register
Registra un nuevo usuario en el sistema.

**Request Body:**
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan@ejemplo.com",
  "password": "MiPassword123!",
  "confirmPassword": "MiPassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "juan@ejemplo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "roles": ["RegularUser"],
  "expiresAt": "2024-01-02T10:30:00Z"
}
```

#### POST /api/auth/login
Autentica un usuario y devuelve un token JWT.

**Request Body:**
```json
{
  "email": "admin@conexa.com",
  "password": "Admin123!"
}
```

### Endpoints de Películas

#### GET /api/movies
Obtiene la lista de todas las películas.
- **Autorización**: Requerida (cualquier usuario autenticado)

#### GET /api/movies/{id}
Obtiene los detalles de una película específica.
- **Autorización**: RegularUser o Administrator

#### POST /api/movies
Crea una nueva película.
- **Autorización**: Solo Administrator

**Request Body:**
```json
{
  "title": "Una Nueva Esperanza",
  "episodeId": 4,
  "openingCrawl": "Es un período de guerra civil...",
  "director": "George Lucas",
  "producer": "Gary Kurtz",
  "releaseDate": "1977-05-25T00:00:00Z",
  "characters": ["Luke Skywalker", "Princess Leia"],
  "planets": ["Tatooine", "Alderaan"],
  "starships": ["Death Star", "Millennium Falcon"],
  "vehicles": ["Sandcrawler"],
  "species": ["Human", "Droid"]
}
```

#### PUT /api/movies/{id}
Actualiza una película existente.
- **Autorización**: Solo Administrator

#### DELETE /api/movies/{id}
Elimina una película.
- **Autorización**: Solo Administrator

#### POST /api/movies/sync
Sincroniza las películas desde la API de Star Wars.
- **Autorización**: Solo Administrator

**Response:**
```json
{
  "message": "Sincronización completada exitosamente",
  "newMoviesSynced": 6,
  "timestamp": "2024-01-01T10:30:00Z"
}
```

## 🔐 Autenticación

Para usar los endpoints protegidos, incluye el token JWT en el header Authorization:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🧪 Ejecutar Pruebas

### Todas las pruebas
```bash
dotnet test
```

### Con cobertura de código
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Pruebas específicas
```bash
dotnet test --filter "ClassName=MovieServiceTests"
```

## 📊 Estructura de la Base de Datos

### Tabla Users (Identity)
- Id (string, PK)
- FirstName (string, 100)
- LastName (string, 100)
- Email (string)
- PasswordHash (string)
- CreatedAt (datetime)
- UpdatedAt (datetime?)

### Tabla Movies
- Id (int, PK)
- Title (string, 200)
- EpisodeId (int, unique)
- OpeningCrawl (text)
- Director (string, 100)
- Producer (string, 200)
- ReleaseDate (datetime)
- Characters (JSON)
- Planets (JSON)
- Starships (JSON)
- Vehicles (JSON)
- Species (JSON)
- StarWarsApiUrl (string?)
- CreatedAt (datetime)
- UpdatedAt (datetime?)

## 🔧 Configuración Avanzada

### Variables de Entorno

Puedes configurar las siguientes variables de entorno:

```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="tu-connection-string"
JwtSettings__SecretKey="tu-secret-key"
JwtSettings__Issuer="tu-issuer"
JwtSettings__Audience="tu-audience"
```

### Logging

Los logs se escriben en:
- **Consola**: Para desarrollo
- **Archivos**: `logs/conexa-starwars-YYYY-MM-DD.txt`

## 🚀 Despliegue

### Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ConexaStarWars.API/ConexaStarWars.API.csproj", "src/ConexaStarWars.API/"]
COPY ["src/ConexaStarWars.Infrastructure/ConexaStarWars.Infrastructure.csproj", "src/ConexaStarWars.Infrastructure/"]
COPY ["src/ConexaStarWars.Core/ConexaStarWars.Core.csproj", "src/ConexaStarWars.Core/"]
RUN dotnet restore "src/ConexaStarWars.API/ConexaStarWars.API.csproj"
COPY . .
WORKDIR "/src/src/ConexaStarWars.API"
RUN dotnet build "ConexaStarWars.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConexaStarWars.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConexaStarWars.API.dll"]
```

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📞 Contacto

- **Desarrollador**: Tu Nombre
- **Email**: tu-email@ejemplo.com
- **LinkedIn**: [tu-perfil](https://linkedin.com/in/tu-perfil)

## 🙏 Agradecimientos

- [Star Wars API](https://swapi.tech/) por proporcionar los datos de las películas
- [Microsoft](https://microsoft.com) por .NET y Entity Framework
- Comunidad de desarrolladores de .NET

---

⭐ ¡No olvides dar una estrella al proyecto si te fue útil! 