namespace Blazor.Core.Weather;

public record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
