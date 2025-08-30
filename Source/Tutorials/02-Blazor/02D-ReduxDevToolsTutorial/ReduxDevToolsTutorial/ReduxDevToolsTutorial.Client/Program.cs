using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReduxDevToolsTutorial.Client;
using ReduxDevToolsTutorial.Client.Services;
using ReduxDevToolsTutorial.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
CommonServicesRegistration.Register(builder.Configuration, builder.Services);

builder.Services.AddSingleton<IWeatherService, WeatherService>();
builder.Services.AddHttpClient(
	name: "Api",
	configureClient: x =>
	{
		x.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
	}
);

await builder.Build().RunAsync();
