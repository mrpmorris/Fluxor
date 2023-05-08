using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;

internal static class DiscoveredMiddlewareClassGenerator
{
	public static Void Generate(SourceProductionContext productionContext, ImmutableArray<DiscoveredMiddlewareClassInfo> classInfos)
	{
		if (classInfos.Length == 0)
			return Void.Value;

		var source = new StringBuilder();
		for(int i = 0; i < classInfos.Length; i++)
		{
			DiscoveredMiddlewareClassInfo classInfo = classInfos[i];
			source.AppendLine($"[assembly: Fluxor.DiscoveredMiddleware(typeof({classInfo.ClassFullName}))]");
		}

		productionContext.AddSource("Fluxor.DiscoveredMiddlewares.cs", source.ToString());

		return Void.Value;
	}
}
