using System.ComponentModel.DataAnnotations;

namespace ConexaStarWars.Core.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int EpisodeId { get; set; }
    public string OpeningCrawl { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Species { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMovieDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID del episodio es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del episodio debe ser mayor a 0")]
    public int EpisodeId { get; set; }

    [Required(ErrorMessage = "El opening crawl es requerido")]
    public string OpeningCrawl { get; set; } = string.Empty;

    [Required(ErrorMessage = "El director es requerido")]
    [MaxLength(100, ErrorMessage = "El director no puede exceder 100 caracteres")]
    public string Director { get; set; } = string.Empty;

    [Required(ErrorMessage = "El productor es requerido")]
    [MaxLength(200, ErrorMessage = "El productor no puede exceder 200 caracteres")]
    public string Producer { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de lanzamiento es requerida")]
    public DateTime ReleaseDate { get; set; }

    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Species { get; set; } = new();
}

public class UpdateMovieDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID del episodio es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del episodio debe ser mayor a 0")]
    public int EpisodeId { get; set; }

    [Required(ErrorMessage = "El opening crawl es requerido")]
    public string OpeningCrawl { get; set; } = string.Empty;

    [Required(ErrorMessage = "El director es requerido")]
    [MaxLength(100, ErrorMessage = "El director no puede exceder 100 caracteres")]
    public string Director { get; set; } = string.Empty;

    [Required(ErrorMessage = "El productor es requerido")]
    [MaxLength(200, ErrorMessage = "El productor no puede exceder 200 caracteres")]
    public string Producer { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de lanzamiento es requerida")]
    public DateTime ReleaseDate { get; set; }

    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Species { get; set; } = new();
}