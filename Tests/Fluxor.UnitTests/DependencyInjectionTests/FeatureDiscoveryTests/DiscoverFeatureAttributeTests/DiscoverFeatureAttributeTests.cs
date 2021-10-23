using Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureAttributeTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureAttributeTests
{
	public class DiscoverFeatureAttributeTests
	{

		[Fact]
		public void WhenFeatureAttributeHasNoParameterValues_ThenFeatureIsCreatedWithDefaultValues()
		{
			IStore store = CreateStore(typeof(StateWithParameterlessFeatureAttribute));
			Assert.Single(store.Features);

			IFeature feature = store.Features.First().Value;
			Assert.Equal(typeof(StateWithParameterlessFeatureAttribute).FullName, feature.GetName());
			Assert.Equal(0, feature.MaximumStateChangedNotificationsPerSecond);
			Assert.Equal(typeof(StateWithParameterlessFeatureAttribute), feature.GetStateType());
			Assert.NotNull(feature.GetState());
		}

		[Fact]
		public void WhenFeatureAttributeHasParameterValues_ThenFeatureIsCreatedWithParameterValues()
		{
			IStore store = CreateStore(typeof(StateWithParameterizedFeatureAttribute));
			Assert.Single(store.Features);

			IFeature feature = store.Features.First().Value;
			Assert.Equal("ParameterizedName", feature.GetName());
			Assert.Equal(42, feature.MaximumStateChangedNotificationsPerSecond);
			Assert.Equal(typeof(StateWithParameterizedFeatureAttribute), feature.GetStateType());
			Assert.NotNull(feature.GetState());
		}

		[Fact]
		public void WhenFeatureHasCreateInitialStateMethodName_ThenMethodIsInvokedToCreateDefaultState()
		{
			IStore store = CreateStore(typeof(StateWithStaticFactoryMethod));
			Assert.Single(store.Features);

			IFeature feature = store.Features.First().Value;
			Assert.Equal(typeof(StateWithStaticFactoryMethod), feature.GetStateType());
			Assert.NotNull(feature.GetState());

			var state = (StateWithStaticFactoryMethod)feature.GetState();
			Assert.Equal(299_792_458, state.SomeValue);
		}

		private static IStore CreateStore(Type typeToScan)
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x.ScanTypes(typeToScan));
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IStore>();
		}
	}
}
