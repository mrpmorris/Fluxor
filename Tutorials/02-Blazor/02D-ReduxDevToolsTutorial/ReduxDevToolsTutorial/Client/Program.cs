using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using System.Net.Http;
using System;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

			builder.Services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.UseReduxDevTools(x =>
				{
					x.Name = "Fluxor ReduxDevTools sample";

					//Example of using Newtonsoft, and optionally providing serialization settings
					//x.UseNewtonsoftJson(_ =>
					//	new Newtonsoft.Json.JsonSerializerSettings
					//	{
					//		ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
					//		{
					//			NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
					//		}
					//	}
					//);

					//Example of using System.Text.Json, and optionally providing serialization options
					//x.UseSystemTextJson(_ =>
					//	new System.Text.Json.JsonSerializerOptions
					//	{
					//		PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
					//	}
					//);
				})
			);
			await builder.Build().RunAsync();
}
	}
}
