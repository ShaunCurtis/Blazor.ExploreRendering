using Blazor.Core;
using Blazor.Core.CQS;
using Blazor.Core.Services;
using Blazor.Core.Weather;
using Blazor.Infrastructure.Weather;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Configuration.WASM;

public static class AppServices
{
    public static void AddAppWASMServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddHttpClient(AppConstants.WeatherHttpClient, client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

        builder.Services.AddScoped<AppScopedService>();
        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherAPIListRequestHandler>();
    }

    public static void AddAppStandAloneServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<AppScopedService>();
        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherStandAloneListRequestHandler>();
    }
}
