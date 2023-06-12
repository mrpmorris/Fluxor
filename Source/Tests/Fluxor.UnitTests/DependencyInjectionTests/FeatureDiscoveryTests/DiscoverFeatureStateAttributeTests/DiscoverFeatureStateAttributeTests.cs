using Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureStateAttributeTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureStateAttributeTests
{
	public class DiscoverFeatureStateAttributeTests
	{

		[Fact]
		public void WhenFeatureStateAttributeHasNoParameterValues_ThenFeatureIsCreatedWithDefaultValues()
		{
			IStore store = CreateStore();
			Assert.Single(store.Features);

			IFeature feature = store.Features.Single(x => x.Value.GetName() == typeof(StateWithParameterlessFeatureStateAttribute).FullName).Value;
			Assert.Equal(0, feature.MaximumStateChangedNotificationsPerSecond);
			Assert.Equal(typeof(StateWithParameterlessFeatureStateAttribute), feature.GetStateType());
			Assert.NotNull(feature.GetState());
		}

		[Fact]
		public void WhenFeatureStateAttributeHasParameterValues_ThenFeatureIsCreatedWithParameterValues()
		{
			IStore store = CreateStore();
			Assert.Single(store.Features);

			IFeature feature = store.Features.Single(x => x.Value.GetName() == typeof(StateWithParameterizedFeatureStateAttribute).FullName).Value;
			Assert.Equal("ParameterizedName", feature.GetName());
			Assert.Equal(42, feature.MaximumStateChangedNotificationsPerSecond);
			Assert.Equal(typeof(StateWithParameterizedFeatureStateAttribute), feature.GetStateType());
			Assert.NotNull(feature.GetState());
		}

		[Fact]
		public void WhenFeatureHasCreateInitialStateMethodName_ThenMethodIsInvokedToCreateDefaultState()
		{
			IStore store = CreateStore();
			Assert.Single(store.Features);

			IFeature feature = store.Features.Single(x => x.Value.GetName() == typeof(StateWithStaticFactoryMethod).FullName).Value;
			Assert.Equal(typeof(StateWithStaticFactoryMethod), feature.GetStateType());
			Assert.NotNull(feature.GetState());

			var state = (StateWithStaticFactoryMethod)feature.GetState();
			Assert.Equal(299_792_458, state.SomeValue);
		}

		private static IStore CreateStore()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x.AddModule<GeneratedFluxorModule>());
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IStore>();
		}
	}
}
