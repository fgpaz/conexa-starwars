namespace ConexaStarWars.Core.DTOs;

public class StarWarsApiResponse
{
    public string Message { get; set; } = string.Empty;
    public int Total_records { get; set; }
    public int Total_pages { get; set; }
    public string? Previous { get; set; }
    public string? Next { get; set; }
    public List<StarWarsFilmResult> Results { get; set; } = new();
}

public class StarWarsFilmResult
{
    public StarWarsFilmProperties Properties { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string _id { get; set; } = string.Empty;
    public int Uid { get; set; }
    public string __v { get; set; } = string.Empty;
}

public class StarWarsFilmProperties
{
    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public List<string> Vehicles { get; set; } = new();
    public List<string> Species { get; set; } = new();
    public string Created { get; set; } = string.Empty;
    public string Edited { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Episode_id { get; set; }
    public string Director { get; set; } = string.Empty;
    public string Release_date { get; set; } = string.Empty;
    public string Opening_crawl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}