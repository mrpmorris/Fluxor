using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorLazyLoading.Client
{
	public partial class App
	{
		private List<Assembly> LazyLoadedAssemblies = new();

		[Inject]
		private LazyAssemblyLoader AssemblyLoader { get; set; }

		private async Task OnNavigateAsync(NavigationContext args)
		{
			Console.WriteLine("Navigating to " + args.Path);
			if (args.Path.StartsWith("Admin", System.StringComparison.OrdinalIgnoreCase))
			{
				IEnumerable<Assembly> assemblies = await AssemblyLoader
					.LoadAssembliesAsync(new[] { "BlazorLazyLoading.AdminModule.dll" })
					.ConfigureAwait(false);
				Console.WriteLine($"Loaded {assemblies.Count()} assemblies");
				LazyLoadedAssemblies.AddRange(assemblies);
			}
		}
	}
}
