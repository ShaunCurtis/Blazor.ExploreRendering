using Blazor.Configuration.WASM;
using Blazr.RenderLogger.WASM;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddRenderStateWASMServices();
builder.AddAppWASMServices();

await builder.Build().RunAsync();
