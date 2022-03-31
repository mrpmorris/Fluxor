using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using Fluxor;
using FluxorBlazorWeb.ReduxDevToolsTutorial.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services.AddFluxor(o =>
{
	o.ScanAssemblies(typeof(Program).Assembly);
#if DEBUG
	o.UseReduxDevTools(rdt =>
	{
		rdt.Name = "Fluxor ReduxDevTools sample";
		rdt.StackTraceLimit = 2;

		// Example of using Newtonsoft, and optionally providing serialization settings
		rdt.UseNewtonsoftJson(_ =>
				new Newtonsoft.Json.JsonSerializerSettings
				{
					ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
					{
						NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
					}
				}
		);

		// Example of using System.Text.Json, and optionally providing serialization options
		// Note: This won't work if RoutingMiddleware is enabled, because
		// System.Text.Json doesn't deserialize classes without a parameterless constructor
		// in .net 3.1
		//rdt.UseSystemTextJson(_ =>
		//	new System.Text.Json.JsonSerializerOptions
		//	{
		//		PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
		//	}
		//);
	});
#endif
});



await builder.Build().RunAsync();
