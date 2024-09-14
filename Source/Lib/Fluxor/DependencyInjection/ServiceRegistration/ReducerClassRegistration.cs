﻿using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.DependencyInjection.ServiceRegistration;

internal static class ReducerClassRegistration
{
	public static void Register(
		IServiceCollection services,
		ReducerClassInfo[] reducerClassInfos,
		FluxorOptions options)
	{
		foreach (ReducerClassInfo reducerClassInfo in reducerClassInfos)
			services.Add(serviceType: reducerClassInfo.ImplementingType, options: options);
	}
}
