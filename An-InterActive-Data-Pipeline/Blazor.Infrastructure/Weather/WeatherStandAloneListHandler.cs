using Blazor.Core.Weather;
using System.Net.Http.Json;

namespace Blazor.Infrastructure.Weather;

public class WeatherStandAloneListRequestHandler : IListRequestHandler<WeatherForecast>
{
    private readonly HttpClient _httpClient;

    public WeatherStandAloneListRequestHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<ListResult<WeatherForecast>> Execute(ListRequest request)
    {
        var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");

        if (forecasts is null)
            return ListResult<WeatherForecast>.Failure($"The server returned no data");

        return ListResult<WeatherForecast>.Success(forecasts, forecasts.Length);
    }
}
