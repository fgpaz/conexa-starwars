using ConexaStarWars.Core.DTOs;
using ConexaStarWars.Core.Entities;

namespace ConexaStarWars.Infrastructure.Mappings;

public static class MovieMapper
{
    public static MovieDto ToDto(Movie movie)
    {
        if (movie == null) return null!;

        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            EpisodeId = movie.EpisodeId,
            OpeningCrawl = movie.OpeningCrawl,
            Director = movie.Director,
            Producer = movie.Producer,
            ReleaseDate = movie.ReleaseDate,
            Characters = movie.Characters ?? new List<string>(),
            Planets = movie.Planets ?? new List<string>(),
            Starships = movie.Starships ?? new List<string>(),
            Vehicles = movie.Vehicles ?? new List<string>(),
            Species = movie.Species ?? new List<string>(),
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt
        };
    }

    public static Movie ToEntity(CreateMovieDto createDto)
    {
        if (createDto == null) return null!;

        return new Movie
        {
            Title = createDto.Title,
            EpisodeId = createDto.EpisodeId,
            OpeningCrawl = createDto.OpeningCrawl,
            Director = createDto.Director,
            Producer = createDto.Producer,
            ReleaseDate = createDto.ReleaseDate,
            Characters = createDto.Characters ?? new List<string>(),
            Planets = createDto.Planets ?? new List<string>(),
            Starships = createDto.Starships ?? new List<string>(),
            Vehicles = createDto.Vehicles ?? new List<string>(),
            Species = createDto.Species ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(Movie movie, UpdateMovieDto updateDto)
    {
        if (movie == null || updateDto == null) return;

        movie.Title = updateDto.Title;
        movie.EpisodeId = updateDto.EpisodeId;
        movie.OpeningCrawl = updateDto.OpeningCrawl;
        movie.Director = updateDto.Director;
        movie.Producer = updateDto.Producer;
        movie.ReleaseDate = updateDto.ReleaseDate;
        movie.Characters = updateDto.Characters ?? new List<string>();
        movie.Planets = updateDto.Planets ?? new List<string>();
        movie.Starships = updateDto.Starships ?? new List<string>();
        movie.Vehicles = updateDto.Vehicles ?? new List<string>();
        movie.Species = updateDto.Species ?? new List<string>();
        movie.UpdatedAt = DateTime.UtcNow;
    }

    public static IEnumerable<MovieDto> ToDtoList(IEnumerable<Movie> movies)
    {
        return movies?.Select(ToDto) ?? Enumerable.Empty<MovieDto>();
    }
} 