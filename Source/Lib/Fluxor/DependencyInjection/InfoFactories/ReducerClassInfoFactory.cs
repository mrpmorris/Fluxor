using Fluxor.DependencyInjection.Generated;
using Fluxor.DependencyInjection.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class ReducerClassInfoFactory
{
	internal static ReducerClassInfo[] Create(
		IServiceCollection services,
		IEnumerable<Type> allCandidateTypes,
		GeneratedRegistrationCatalog catalog)
	=>
		allCandidateTypes
			.Where(t => t != typeof(ReducerWrapper<,>))
			.Select(t =>
				new
				{
					ImplementingType = t,
					StateType = GetStateType(t, catalog)
				})
			.Where(x => x.StateType is not null)
			.Select(x => new ReducerClassInfo(
				implementingType: x.ImplementingType,
				stateType: x.StateType))
			.ToArray();

	private static Type GetStateType(Type type, GeneratedRegistrationCatalog catalog) =>
		catalog.TryGetEntry(type, out GeneratedTypeEntry entry)
		? entry.ReducerClassStateType
		: TypeHelper.GetGenericParametersForImplementedInterface(type, typeof(IReducer<>))?[0];
}
