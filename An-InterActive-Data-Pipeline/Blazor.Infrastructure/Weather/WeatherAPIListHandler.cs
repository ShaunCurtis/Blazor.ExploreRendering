using Blazor.Core;
using Blazor.Core.Weather;
using System.Net.Http.Json;

namespace Blazor.Infrastructure.Weather;

public class WeatherAPIListRequestHandler : IListRequestHandler<WeatherForecast>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherAPIListRequestHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask<ListResult<WeatherForecast>> Execute(ListRequest request)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.WeatherHttpClient);

        var httpResult = await http.PostAsJsonAsync<ListRequest>(AppConstants.WeatherForecastListAPIUrl, request);

        if (!httpResult.IsSuccessStatusCode)
            return ListResult<WeatherForecast>.Failure($"The server returned a status code of : {httpResult.StatusCode}");

        var listResult = await httpResult.Content.ReadFromJsonAsync<ListResult<WeatherForecast>>();

        return listResult ?? ListResult<WeatherForecast>.Failure($"No data was returned");
    }
}
