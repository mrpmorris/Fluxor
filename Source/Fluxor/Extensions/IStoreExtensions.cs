using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.Extensions
{

	internal static class IStoreExtensions
	{
		// TODO: PeteM - Needs tests
		public static IFeature GetFeatureByStateType(this IStore store, Type stateType)
		{
			IFeature[] compatibleFeatures = store.Features
				.Select(x => x.Value)
				.Where(x => x.GetStateType() == stateType)
				.ToArray();

			if (compatibleFeatures.Length == 0)
				throw new KeyNotFoundException(
					$"Store does not contain a feature with state type '{stateType.FullName}'");
			if (compatibleFeatures.Length > 1)
				throw new KeyNotFoundException(
					$"Store contains more than one feature with state type '{stateType.FullName}'");

			return compatibleFeatures[0];
		}

		public static IFeature<TState> GetFeatureByStateType<TState>(this IStore store) =>
			(IFeature<TState>)GetFeatureByStateType(store, typeof(TState));
	}
}
