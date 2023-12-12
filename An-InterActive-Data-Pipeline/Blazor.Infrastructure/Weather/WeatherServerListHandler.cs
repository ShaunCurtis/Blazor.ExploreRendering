using Blazor.Core.Weather;

namespace Blazor.Infrastructure.Weather;

public class WeatherServerListRequestHandler : IListRequestHandler<WeatherForecast>
{
    public async ValueTask<ListResult<WeatherForecast>> Execute(ListRequest request)
    {
        return await WeatherProvider.GetForecasts(request);
    }
}
