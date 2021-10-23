using Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests
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
				if (assemblyToScan != null)
				{
					x.ScanAssemblies(assemblyToScan);
					// Allow all features in this namepspace to be scanned
					x.AddMiddleware<IsolatedTests>();
				}
				if (typeToScan != null)
					x.ScanTypes(typeToScan);
			});
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IStore result = serviceProvider.GetRequiredService<IStore>();
			return result;
		}
	}
}
