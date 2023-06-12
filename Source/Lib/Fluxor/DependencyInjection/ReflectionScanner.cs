using Fluxor.DependencyInjection.InfoFactories;
using Fluxor.DependencyInjection.ServiceRegistration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal static class ReflectionScanner
	{
		internal static void Scan(
			this IServiceCollection services,
			FluxorOptions options,
			IEnumerable<IFluxorModule> modulesToImport,
			IEnumerable<AssemblyScanSettings> scanIncludeList)
		{
			modulesToImport ??= Array.Empty<IFluxorModule>();

			if (!modulesToImport.Any())
				return;

			// Find all concrete implementors of IReducer<T>
			ReducerClassInfo[] reducerClassInfos =
				ReducerClassInfoFactory.Create(modulesToImport.SelectMany(x => x.Reducers));

			// Find all concrete implementors of IEffect<T>
			EffectClassInfo[] effectClassInfos =
				EffectClassInfoFactory.Create(modulesToImport.SelectMany(x => x.Effects));

			// TODO: PeteM - I need the FeatureStateAttribute infos, not the types
			FeatureStateInfo[] featureStateInfos =
				FeatureStateInfoFactory.Create(services, modulesToImport.SelectMany(x => x.Features));

			StoreRegistration.Register(
				services,
				options,
				featureStateInfos,
				reducerClassInfos,
				effectClassInfos,
				modulesToImport.SelectMany(x => x.Dependencies));
		}
	}
}
