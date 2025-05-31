# Usar la imagen base de ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen del SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["src/ConexaStarWars.API/ConexaStarWars.API.csproj", "src/ConexaStarWars.API/"]
COPY ["src/ConexaStarWars.Infrastructure/ConexaStarWars.Infrastructure.csproj", "src/ConexaStarWars.Infrastructure/"]
COPY ["src/ConexaStarWars.Core/ConexaStarWars.Core.csproj", "src/ConexaStarWars.Core/"]

RUN dotnet restore "src/ConexaStarWars.API/ConexaStarWars.API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/src/ConexaStarWars.API"
RUN dotnet build "ConexaStarWars.API.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "ConexaStarWars.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ConexaStarWars.API.dll"] 