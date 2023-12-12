using Blazr.RenderLogger.WASM;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddRenderStateWASMServices();

await builder.Build().RunAsync();
