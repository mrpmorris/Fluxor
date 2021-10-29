using Fluxor.DependencyInjection.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class ReducerClassInfoFactory
	{
		internal static ReducerClassInfo[] Create(
			IServiceCollection services,
			IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Where(t => t != typeof(ReducerWrapper<,>))
				.Select(t =>
					new
					{
						ImplementingType = t,
						GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IReducer<>))
					})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new ReducerClassInfo(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]))
				.ToArray();
	}
}
