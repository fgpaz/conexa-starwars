using System.ComponentModel.DataAnnotations;

namespace ConexaStarWars.Core.Entities;

public class Movie
{
    public int Id { get; set; }

    [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;

    public int EpisodeId { get; set; }

    [Required] public string OpeningCrawl { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string Director { get; set; } = string.Empty;

    [Required] [MaxLength(200)] public string Producer { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Propiedades adicionales de la API de Star Wars
    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Species { get; set; } = new();

    // URL original de la API de Star Wars
    public string? StarWarsApiUrl { get; set; }
}