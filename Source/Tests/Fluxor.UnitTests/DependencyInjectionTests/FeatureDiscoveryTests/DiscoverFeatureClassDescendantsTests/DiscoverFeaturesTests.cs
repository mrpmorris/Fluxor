using Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureClassDescendantsTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureClassDescendantsTests
{
	public class DiscoverFeaturesTests
	{
		[Fact]
		public void WhenFeatureIsIncludedViaScanAssemblies_ThenItIsAddedToTheStore()
		{
			IStore store = CreateStore(
				typeof(IsolatedTests).Assembly,
				typeToScan: null);
			Assert.Equal(2, store.Features.Count);
		}

		[Theory]
		[InlineData(typeof(IntegerFeature))]
		public void WhenFeatureIsScannedViaType_ThenItIsAddedToTheStore(Type typeToScan)
		{
			IStore store = CreateStore(
				assemblyToScan: null,
				typeToScan);
			Assert.Single(store.Features);
			Assert.Equal(typeToScan.Name, store.Features.Single().Key);
		}

		private static IStore CreateStore(
			Assembly assemblyToScan,
			Type typeToScan)
		{
			var services = new ServiceCollection();
			services.AddFluxor(x =>
			{
				if (assemblyToScan is not null)
				{
					x.AddModule<GeneratedFluxorModule>();
					// Allow all features in this namepspace to be scanned
					x.AddMiddleware<IsolatedTests>();
				}
				if (typeToScan is not null)
					x.ScanTypes(typeToScan);
			});
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IStore result = serviceProvider.GetRequiredService<IStore>();
			return result;
		}
	}
}
