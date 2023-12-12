using Blazor.Core.CQS;
using Blazor.Core.Weather;

namespace Blazor.Infrastructure.Weather;

public static class WeatherProvider
{
    private static List<WeatherForecast>? _forecasts;

    public static async ValueTask<ListResult<WeatherForecast>> GetForecasts(ListRequest request)
    {
        // Fake an async database transaction
        await Task.Yield();

        _forecasts ??= GetForecasts();

        var query = _forecasts;
 
        var listQuery = query
            .Skip(request.StartIndex)
            .Take(request.PageSize);

        var totalCount = query.Count();

        return ListResult<WeatherForecast>.Success(listQuery, totalCount);
    }

    private static List<WeatherForecast> GetForecasts()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        return Enumerable.Range(1, 1000).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();
    }
}
