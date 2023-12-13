In this section I demonstrate how to build a data pipeline that can be used in both Server and Client Side contexts.

I apply Clean Design principles and various modern patterns to the weather.  The point of the exercise is to demonstrate how to create a structured approach to the data pipeline that crosses the server/client divide and can be used in most deployment contexts.

## Projects

The simplest way to manage and enforce *Clean Design* dependencies is to use multiple projects.  It's all too easy in a directory based approach to breach boundaries without realising you're doing it.

The base template is *Blazor Web App* with *Interactive Auto* and *Global* options selected.  This gives you two deployment projects.

There are four library projects:

1. *Blazor.Core* - contains the domain objects, the [very small] core business logic and the Infrastructure contracts [Interfaces].  There are no project dependencies.
2. *Blazor.Infrastructure* - contains the contract implementations connecting the application to outside resources.
3. *Blazor.Configuration* - contains the static configurstion methods for the Server deployment projects.
4. *Blazor.Configuration.WASM* - contains the static configurstion methods for the WASM Client deployment projects.  These are seperate as there are conflicts with server packages.

The  Client ans Server projects represent the deployment scenarios available though the Templates.

## Data Provider

First a data provider for the weather data. It's an infrastructure class residing in *Blazor.Infrastructure*.

`WeatherProvider` is a static class that encapsulates the code from the `Weather` page. 

```csharp
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
        return Enumerable.Range(1, 100).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();
    }
}
```

## Requests, Results and Handlers

All requests into the data pipeline are made with *Request* objects.  This is the `ListRequest`.  

> You should never request an unconfined list: you have no idea how many records you wiil get back!  
 
The base classes and interfaces are part of the core application logic so reside in *Blazor.Core*.  The structure I'm implementing is based on the *CQS Pattern*. 

```csharp
namespace Blazor.Core.CQS;

public record ListRequest
{
    public int StartIndex { get; init; }
    public int PageSize { get; init; }

    public ListRequest()
    {
        StartIndex = 0;
        PageSize = 1000;
    }

    public static ListRequest Create(int startIndex, int pageSize)
        => new() { StartIndex = startIndex, PageSize= pageSize };
}
```

All requests return *Result* objects.  This is the `ListResult`.

```csharp
namespace Blazor.Core.CQS;

public record ListResult<TItem>
{
    public IEnumerable<TItem>? Items { get; init; }
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public int TotalCount { get; init; }

    public ListResult()
    {
        Successful = false;
    }

    public static ListResult<TItem> Success(IEnumerable<TItem> items, int totalCount)
        => new() { Items = items, Successful = true, TotalCount = totalCount };

    public static ListResult<TItem> Failure(string message)
        => new() { Message = message };
}
```

Requests are handled by *Handlers*, defined by the `IListRequestHandler` interface.

```csharp
namespace Blazor.Core.CQS;

public interface IListRequestHandler<TItem>
{
    public ValueTask<ListResult<TItem>> Execute(ListRequest request);
}
```

## Handler implementations

There are multiple implementations of the `IListRequestHandler` for each weather forecast context.

### Server Implementation

This interfaces directly with the data provider. In a real database context, this would interface with the *ORM* such as Entity Framework or Dapper.

It's used in the Blazor server context and the API server context.

```csharp
using Blazor.Core.Weather;

namespace Blazor.Infrastructure.Weather;

public class WeatherServerListRequestHandler : IListRequestHandler<WeatherForecast>
{
    public async ValueTask<ListResult<WeatherForecast>> Execute(ListRequest request)
    {
        return await WeatherProvider.GetForecasts(request);
    }
}
```

### API Implementation

The API implementation uses the `IHttpClientFactory`, and a named instance.  I'll look at the services we need to register shortly.

```csharp
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

        var httpResult = await http.PostAsJsonAsync<ListRequest>("/API/Weather/GetForecasts", request);

        if (!httpResult.IsSuccessStatusCode)
            return ListResult<WeatherForecast>.Failure($"The server returned a status code of : {httpResult.StatusCode}");

        var listResult = await httpResult.Content.ReadFromJsonAsync<ListResult<WeatherForecast>>();

        return listResult ?? ListResult<WeatherForecast>.Failure($"No data was returned");
    }
}
```

### StandAlone Implementation

To demonstrate the versitility of this approach I've encapsulated the StandAlone template's call to the static server side json file into an implementation we can use in the StandAlone project.

```csharp
using Blazor.Core.CQS;
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
```

## The WeatherList Component

Add a *WeatherList.razor* component to the *Blazor.Component* library.

I've switched to a `Virtualize` implementation that plugs into the data pipeline we're created.  The `Virtualize` component is now responsible for managing getting data through the injected `IListRequestHandler<WeatherForecast>`.  The actual handler that gets injected into the component will depend on which implementation is registered in the services for the application endpoint.

The component uses the `IRenderStateService` to detect when pre-rendering. 

```csharp
@using Blazor.Core.Weather
@using Blazor.Core.CQS;
@using Microsoft.AspNetCore.Components.Web.Virtualization
@inject IListRequestHandler<WeatherForecast> ListHandler
@inject IRenderStateService RenderState

<PageTitle>Weather</PageTitle>

<h1>Weather</h1>

<p>This component demonstrates showing data.</p>

@if(this.RenderState.IsPreRender)
{
    <div>Loading...</div>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temp. (C)</th>
                <th>Temp. (F)</th>
                <th>Summary</th>
            </tr>
        </thead>
        <tbody>
            <Virtualize Context="forecast" ItemsProvider="this.GetRowsAsync">
                <tr>
                    <td>@forecast.Date.ToShortDateString()</td>
                    <td>@forecast.TemperatureC</td>
                    <td>@forecast.TemperatureF</td>
                    <td>@forecast.Summary</td>
                </tr>
            </Virtualize>
        </tbody>
    </table>
}

@code {

    private async ValueTask<ItemsProviderResult<WeatherForecast>> GetRowsAsync(
    ItemsProviderRequest request)
    {
        var result = await ListHandler.Execute(ListRequest.Create(request.StartIndex, request.Count));

        if (result.Successful && result.Items is not null)
            return new ItemsProviderResult<WeatherForecast>(result.Items, result.TotalCount);

        return new ItemsProviderResult<WeatherForecast>(Enumerable.Empty<WeatherForecast>(), 0);
    }
}
```

### The Configuration Projects

The sole purpose of *Blazor.Configuration* and *Blazor.Configuration.WASM* are to provide configuration extensions for the deployment projects.

They shield the deployment projects from dependency on the *Blazor.Infrastructure* domain.

There are now three extension methods to choode from when registering the application services.

*Blazor.Configuration* contains the extensions for adding the server services and the API endpoints.

```csharp
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
        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherServerListRequestHandler>();
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
```

*Blazor.Configuration.WASM* contains the extensions for adding the WASM services.

```csharp
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
    public static void AddAppWASMInfrastructureServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddHttpClient(AppConstants.WeatherHttpClient, client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherAPIListRequestHandler>();
    }

    public static void AddAppStandAloneInfrastructureServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<IListRequestHandler<WeatherForecast>, WeatherStandAloneListRequestHandler>();
    }
}
```

### AppConstants

This simple static `AppConstants` class to holds the http client name and API Urls.

```csharp
namespace Blazor.Core;

public static class AppConstants
{
    public const string WeatherHttpClient = "WeatherHttpClient";
    public const string WeatherForecastListAPIUrl = "/API/Weather/GetForecasts";
    
}
```

### Configuring the Deployment Projects

The Server project `Program` adds the necessary services and API endpoints.

```csharp
using Blazor.Client.Pages;
using Blazor.Components;
using Blazor.Configuration;
using Blazr.RenderLogger.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.AddRenderStateServerServices();
builder.AddAppServerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.AddAppAPIEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
```

The Client peoject adds the services.

```caharp
using Blazor.Configuration.WASM;
using Blazr.RenderLogger.WASM;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddRenderStateWASMServices();
builder.AddAppWASMServices();

await builder.Build().RunAsync();
```