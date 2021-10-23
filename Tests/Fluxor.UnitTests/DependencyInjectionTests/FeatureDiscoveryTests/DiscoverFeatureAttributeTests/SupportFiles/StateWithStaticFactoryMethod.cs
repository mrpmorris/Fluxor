﻿namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureAttributeTests.SupportFiles
{
	[Feature(CreateInitialStateMethodName = nameof(CreateInitialState))]
	public class StateWithStaticFactoryMethod
	{
		public int SomeValue { get; private set; }

		public StateWithStaticFactoryMethod(int someValue)
		{
			SomeValue = someValue;
		}

		public static StateWithStaticFactoryMethod CreateInitialState() =>
			new StateWithStaticFactoryMethod(someValue: 299_792_458);
	}
}
