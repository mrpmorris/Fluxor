using System.Reflection;
using Fluxor;
using Fluxor.Performance.BlazorWasm;
using Fluxor.Performance.Sample;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

BenchmarkMarks.Mark(BenchmarkMarks.Main);
WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// An app's own assembly is loaded before it calls AddFluxor, so the sample is too
Assembly sampleAssembly = typeof(State000).Assembly;

BenchmarkMarks.Mark(BenchmarkMarks.BeforeAddFluxor);
builder.Services.AddFluxor(options => options.ScanAssemblies(sampleAssembly));
BenchmarkMarks.Mark(BenchmarkMarks.AfterAddFluxor);

WebAssemblyHost host = builder.Build();
BenchmarkMarks.Mark(BenchmarkMarks.AfterBuild);

await host.RunAsync();
