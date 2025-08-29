using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using FluxorBlazorWeb.ReduxDevToolsTutorial;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluxor(x =>
{
	x.ScanAssemblies(typeof(Program).Assembly);
	x.UseRouting(); // Add this to support routing via Fluxor actions
#if DEBUG
	x.UseReduxDevTools();
#endif
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
