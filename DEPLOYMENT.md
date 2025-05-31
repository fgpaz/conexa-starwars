# Guía de Deployment - Conexa Star Wars API

Esta guía proporciona instrucciones detalladas para desplegar la aplicación Conexa Star Wars API en diferentes entornos y plataformas.

## 📋 Tabla de Contenidos

- [Prerrequisitos](#prerrequisitos)
- [Configuración de Entornos](#configuración-de-entornos)
- [Deployment Local](#deployment-local)
- [Deployment con Docker](#deployment-con-docker)
- [Deployment en Azure](#deployment-en-azure)
- [Deployment en AWS](#deployment-en-aws)
- [Deployment en IIS](#deployment-en-iis)
- [Variables de Entorno](#variables-de-entorno)
- [Monitoreo y Logs](#monitoreo-y-logs)
- [Troubleshooting](#troubleshooting)

## 🔧 Prerrequisitos

### Desarrollo Local
- .NET 8 SDK
- Visual Studio 2022 o VS Code
- SQL Server (opcional, usa InMemory por defecto)

### Producción
- .NET 8 Runtime
- SQL Server 2019+ o Azure SQL Database
- IIS 10+ (para Windows Server)
- Docker (opcional)

## ⚙️ Configuración de Entornos

### appsettings.json (Desarrollo)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ConexaStarWarsDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "ConexaStarWarsSecretKeyForJWTTokenGeneration2024!",
    "Issuer": "ConexaStarWarsAPI",
    "Audience": "ConexaStarWarsClient",
    "ExpirationHours": 24
  }
}
```

### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=ConexaStarWarsDb;User Id=your-user;Password=your-password;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_PRODUCTION_SECRET_KEY_HERE_MINIMUM_32_CHARACTERS",
    "Issuer": "ConexaStarWarsAPI",
    "Audience": "ConexaStarWarsClient",
    "ExpirationHours": 8
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🏠 Deployment Local

### 1. Clonar y Configurar
```bash
git clone https://github.com/tu-usuario/conexa-starwars.git
cd conexa-starwars
dotnet restore
```

### 2. Configurar Base de Datos (Opcional)
```bash
# Para usar SQL Server local
dotnet ef database update --project src/ConexaStarWars.Infrastructure --startup-project src/ConexaStarWars.API
```

### 3. Ejecutar la Aplicación
```bash
cd src/ConexaStarWars.API
dotnet run --environment Development
```

La aplicación estará disponible en:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

## 🐳 Deployment con Docker

### 1. Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ConexaStarWars.API/ConexaStarWars.API.csproj", "src/ConexaStarWars.API/"]
COPY ["src/ConexaStarWars.Core/ConexaStarWars.Core.csproj", "src/ConexaStarWars.Core/"]
COPY ["src/ConexaStarWars.Infrastructure/ConexaStarWars.Infrastructure.csproj", "src/ConexaStarWars.Infrastructure/"]
RUN dotnet restore "src/ConexaStarWars.API/ConexaStarWars.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/ConexaStarWars.API"
RUN dotnet build "ConexaStarWars.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ConexaStarWars.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "ConexaStarWars.API.dll"]
```

### 2. docker-compose.yml
```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=ConexaStarWarsDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sqlserver
    networks:
      - conexa-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - conexa-network

volumes:
  sqlserver_data:

networks:
  conexa-network:
    driver: bridge
```

### 3. Comandos Docker
```bash
# Construir y ejecutar
docker-compose up --build

# Solo ejecutar (si ya está construido)
docker-compose up

# Ejecutar en background
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener
docker-compose down
```

## ☁️ Deployment en Azure

### 1. Azure App Service

#### Usando Azure CLI
```bash
# Login
az login

# Crear grupo de recursos
az group create --name conexa-starwars-rg --location "East US"

# Crear App Service Plan
az appservice plan create --name conexa-starwars-plan --resource-group conexa-starwars-rg --sku B1 --is-linux

# Crear Web App
az webapp create --resource-group conexa-starwars-rg --plan conexa-starwars-plan --name conexa-starwars-api --runtime "DOTNETCORE:8.0"

# Configurar variables de entorno
az webapp config appsettings set --resource-group conexa-starwars-rg --name conexa-starwars-api --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  JwtSettings__SecretKey="YOUR_PRODUCTION_SECRET_KEY_HERE" \
  ConnectionStrings__DefaultConnection="YOUR_AZURE_SQL_CONNECTION_STRING"

# Deploy desde GitHub
az webapp deployment source config --resource-group conexa-starwars-rg --name conexa-starwars-api --repo-url https://github.com/tu-usuario/conexa-starwars --branch main --manual-integration
```

#### Usando Azure Portal
1. Crear nuevo App Service
2. Seleccionar .NET 8 como runtime
3. Configurar variables de entorno en Configuration
4. Configurar deployment desde GitHub

### 2. Azure SQL Database
```bash
# Crear SQL Server
az sql server create --name conexa-starwars-server --resource-group conexa-starwars-rg --location "East US" --admin-user sqladmin --admin-password "YourStrong@Passw0rd"

# Crear base de datos
az sql db create --resource-group conexa-starwars-rg --server conexa-starwars-server --name ConexaStarWarsDb --service-objective Basic

# Configurar firewall
az sql server firewall-rule create --resource-group conexa-starwars-rg --server conexa-starwars-server --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

## 🚀 Deployment en AWS

### 1. Elastic Beanstalk

#### Preparar para deployment
```bash
# Publicar aplicación
dotnet publish -c Release -o ./publish

# Crear archivo de configuración
# .ebextensions/01-aspnetcore.config
```

#### .ebextensions/01-aspnetcore.config
```yaml
option_settings:
  aws:elasticbeanstalk:container:dotnet:apppool:
    Target Framework: net8.0
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    JwtSettings__SecretKey: YOUR_PRODUCTION_SECRET_KEY_HERE
```

#### Deployment
```bash
# Instalar EB CLI
pip install awsebcli

# Inicializar
eb init

# Crear entorno
eb create conexa-starwars-prod

# Deploy
eb deploy
```

### 2. ECS con Fargate
```yaml
# task-definition.json
{
  "family": "conexa-starwars",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "executionRoleArn": "arn:aws:iam::ACCOUNT:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "conexa-starwars-api",
      "image": "your-account.dkr.ecr.region.amazonaws.com/conexa-starwars:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/conexa-starwars",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## 🖥️ Deployment en IIS

### 1. Preparar el Servidor
```powershell
# Instalar IIS y ASP.NET Core Module
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, IIS-StaticContent, IIS-DefaultDocument, IIS-DirectoryBrowsing

# Descargar e instalar .NET 8 Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. Publicar la Aplicación
```bash
# Publicar para IIS
dotnet publish -c Release -o ./publish --self-contained false --runtime win-x64

# Copiar archivos al servidor IIS
# Ejemplo: C:\inetpub\wwwroot\conexa-starwars
```

### 3. Configurar IIS
```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\ConexaStarWars.API.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### 4. Configurar Application Pool
```powershell
# Crear Application Pool
New-WebAppPool -Name "ConexaStarWarsPool"
Set-ItemProperty -Path "IIS:\AppPools\ConexaStarWarsPool" -Name "managedRuntimeVersion" -Value ""

# Crear sitio web
New-Website -Name "ConexaStarWars" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\conexa-starwars" -ApplicationPool "ConexaStarWarsPool"
```

## 🔐 Variables de Entorno

### Variables Requeridas
```bash
# Entorno
ASPNETCORE_ENVIRONMENT=Production

# Base de datos
ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..."

# JWT
JwtSettings__SecretKey="YOUR_SECRET_KEY_MINIMUM_32_CHARACTERS"
JwtSettings__Issuer="ConexaStarWarsAPI"
JwtSettings__Audience="ConexaStarWarsClient"
JwtSettings__ExpirationHours="8"

# URLs (opcional)
ASPNETCORE_URLS="https://+:443;http://+:80"
```

### Variables Opcionales
```bash
# Logging
Logging__LogLevel__Default="Warning"
Logging__LogLevel__Microsoft="Warning"

# CORS (si es necesario)
AllowedOrigins="https://yourdomain.com,https://www.yourdomain.com"
```

## 📊 Monitoreo y Logs

### 1. Serilog Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/conexa-starwars-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### 2. Health Checks
```csharp
// En Program.cs
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://swapi.tech/api/films"), "Star Wars API");

app.MapHealthChecks("/health");
```

### 3. Application Insights (Azure)
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

## 🔧 Troubleshooting

### Problemas Comunes

#### 1. Error de Conexión a Base de Datos
```bash
# Verificar connection string
# Verificar que SQL Server esté ejecutándose
# Verificar permisos de usuario
```

#### 2. Error 500.30 - ASP.NET Core app failed to start
```bash
# Verificar que .NET 8 Runtime esté instalado
# Verificar logs en Event Viewer (Windows)
# Verificar permisos de archivos
```

#### 3. JWT Token Invalid
```bash
# Verificar que SecretKey tenga al menos 32 caracteres
# Verificar configuración de Issuer y Audience
# Verificar fecha de expiración
```

#### 4. CORS Errors
```csharp
// Configurar CORS en Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

### Logs Útiles
```bash
# Ver logs de aplicación
tail -f logs/conexa-starwars-*.txt

# Ver logs de Docker
docker logs conexa-starwars-api

# Ver logs de IIS
# Event Viewer > Windows Logs > Application
```

### Comandos de Diagnóstico
```bash
# Verificar estado de la aplicación
curl https://your-domain.com/health

# Verificar endpoints
curl https://your-domain.com/api/movies

# Verificar SSL
openssl s_client -connect your-domain.com:443
```

## 📝 Checklist de Deployment

### Pre-deployment
- [ ] Configurar variables de entorno de producción
- [ ] Actualizar connection strings
- [ ] Configurar JWT secret key seguro
- [ ] Revisar configuración de CORS
- [ ] Ejecutar pruebas unitarias
- [ ] Verificar configuración de logging

### Post-deployment
- [ ] Verificar que la aplicación inicie correctamente
- [ ] Probar endpoints de autenticación
- [ ] Probar endpoints de películas
- [ ] Verificar logs de aplicación
- [ ] Probar sincronización con API de Star Wars
- [ ] Configurar monitoreo y alertas
- [ ] Documentar URLs y credenciales

---

Para más información o soporte, consulta la documentación oficial de .NET Core deployment o contacta al equipo de desarrollo. 