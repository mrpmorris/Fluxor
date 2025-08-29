using Fluxor;
using FluxorBlazorWeb.MiddlewareTutorial;
using FluxorBlazorWeb.MiddlewareTutorial.Store.Middlewares.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluxor(x => x
	.ScanAssemblies(typeof(Program).Assembly)
	.AddMiddleware<LoggingMiddleware>());

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
