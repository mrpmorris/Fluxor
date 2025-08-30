using ReduxDevToolsTutorial.Client;
using ReduxDevToolsTutorial.Components;
using ReduxDevToolsTutorial.Contracts;
using ReduxDevToolsTutorial.Services;

var builder = WebApplication.CreateBuilder(args);
CommonServicesRegistration.Register(builder.Configuration, builder.Services);
builder.Services.AddSingleton<IWeatherService, WeatherService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

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

app.MapGet(
    pattern: "/api/weather",
    handler: (IWeatherService weatherService) => weatherService.GetForecastsAsync()
);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ReduxDevToolsTutorial.Client._Imports).Assembly);

app.Run();
