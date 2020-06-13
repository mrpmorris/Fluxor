using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace FluxorBlazorWeb.EffectsTutorial.Server
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class Startup
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddResponseCompression(opts =>
			{
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
									new[] { "application/octet-stream" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				// app.UseBlazorDebugging();
				app.UseWebAssemblyDebugging();
			}

			app.UseStaticFiles();
			// Replace the call to app.UseClientSideBlazorFiles<Client.Program>() with app.UseBlazorFrameworkFiles()
			// app.UseClientSideBlazorFiles<Client.Program>();
			app.UseBlazorFrameworkFiles();
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
				// Replace the call to endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html") 
				// with endpoints.MapFallbackToFile("index.html").
				// endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}
