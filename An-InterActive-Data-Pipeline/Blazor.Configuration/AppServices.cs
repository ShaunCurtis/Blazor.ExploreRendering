using Blazor.Core;
using Blazor.Core.CQS;
using Blazor.Core.Services;
using Blazor.Core.Weather;
using Blazor.Infrastructure.Weather;
using Microsoft.AspNetCore.Mvc;

namespace Blazor.Configuration;

public static class AppServices
{
    public static void AddAppServerServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AppScopedService>();
        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherServerListRequestHandler>();
        builder.Services.AddHttpContextAccessor();
    }

    public static void AddAppAPIEndpoints(this WebApplication app)
    {
        app.MapPost(AppConstants.WeatherForecastListAPIUrl, async ([FromBody] ListRequest request, IListRequestHandler<WeatherForecast> handler) =>
        {
            var result = await handler.Execute(request);
            return Results.Ok(result);
        });
    }
}
