using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// Serves a published <c>wwwroot</c> as static files from Kestrel on a free loopback port. Files are served as they
/// are (no compression negotiation) and marked not to be cached.
/// </summary>
internal sealed class StaticFileHost : IAsyncDisposable
{
	private readonly WebApplication App;

	public string Url { get; }

	private StaticFileHost(WebApplication app, string url)
	{
		App = app;
		Url = url;
	}

	public static async Task<StaticFileHost> StartAsync(string root)
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = root });
		builder.Logging.ClearProviders();
		builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
		WebApplication app = builder.Build();

		var files = new PhysicalFileProvider(root);
		var contentTypes = new FileExtensionContentTypeProvider();
		contentTypes.Mappings[".wasm"] = "application/wasm";
		contentTypes.Mappings[".dat"] = "application/octet-stream";
		contentTypes.Mappings[".pdb"] = "application/octet-stream";
		app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = files,
			ContentTypeProvider = contentTypes,
			ServeUnknownFileTypes = true,
			DefaultContentType = "application/octet-stream",
			OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-store"
		});

		await app.StartAsync();
		string url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
		return new StaticFileHost(app, url.TrimEnd('/') + "/");
	}

	public async ValueTask DisposeAsync()
	{
		await App.StopAsync();
		await App.DisposeAsync();
	}
}
