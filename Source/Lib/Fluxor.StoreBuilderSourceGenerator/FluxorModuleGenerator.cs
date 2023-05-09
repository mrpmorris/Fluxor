using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace Fluxor.StoreBuilderSourceGenerator;

internal class FluxorModuleGenerator
{
	internal static void Generate(SourceProductionContext productionContext, string rootNamespace, DiscoveredClasses discoveredClasses)
	{
		string sourceCode = GenerateSourceCode(rootNamespace, discoveredClasses);
		productionContext.AddSource($"FluxorModule.cs", sourceCode);
	}

	private static string GenerateSourceCode(string rootNamespace, DiscoveredClasses discoveredClasses)
	{
		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		bool hasNamespace = rootNamespace is not null && rootNamespace.Length > 0;

		writer.WriteLine("using System.Collections.Immutable;");
		if (hasNamespace)
		{
			writer.WriteLine($"namespace {rootNamespace}");
			writer.WriteLine("{");
			writer.Indent++;
		}

		GenerateClassSource(writer, discoveredClasses);

		if (hasNamespace)
		{
			writer.Indent--;
			writer.WriteLine("}");
		}
		writer.Flush();
		return result.ToString();
	}

	private static void GenerateClassSource(IndentedTextWriter writer, DiscoveredClasses discoveredClasses)
	{
		string[] generatedEffectClassNames = discoveredClasses.EffectMethodInfos.Select(EffectGenerator.GetGeneratedClassName).ToArray();
		string[] generatedFeatureClassNames = discoveredClasses.FeatureStateClassInfos.Select(FeatureGenerator.GetGeneratedClassName).ToArray();
		string[] generatedReducerClassNames = discoveredClasses.ReducerMethodInfos.Select(ReducerGenerator.GetGeneratedClassName).ToArray();
		string[] dependencies = discoveredClasses
			.EffectMethodInfos
			.Where(x => !x.IsStatic)
			.Select(x => x.ClassFullName)
			.Distinct()
			.ToArray();

		string[] effectClassNames = generatedEffectClassNames.Union(discoveredClasses.DiscoveredEffectClassNames).Distinct().ToArray();
		string[] featureClassNames = generatedFeatureClassNames.Union(discoveredClasses.DiscoveredFeatureClassNames).Distinct().ToArray();
		string[] middlewareClassNames = discoveredClasses.DiscoveredMiddlewareClassNames.ToArray();
		string[] reducerClassNames = generatedReducerClassNames.Union(discoveredClasses.DiscoveredReducerClassNames).Distinct().ToArray();

		writer.WriteLine("public static class FluxorModule");
		using (writer.CodeBlock())
		{
			GenerateClassArrayPropertySource(writer, "Dependencies", dependencies);
			GenerateClassArrayPropertySource(writer, "Effects", effectClassNames);
			GenerateClassArrayPropertySource(writer, "Features", featureClassNames);
			GenerateClassArrayPropertySource(writer, "Middlewares", middlewareClassNames);
			GenerateClassArrayPropertySource(writer, "Reducers", reducerClassNames);
		}
	}

	private static void GenerateClassArrayPropertySource(IndentedTextWriter writer, string propertyName, string[] classes)
	{
		writer.WriteLine($"public static readonly ImmutableArray<Type> {propertyName} = new Type[] {{");
		writer.Indent++;
		for(int i = 0; i < classes.Length; i++)
		{
			string className = classes[i];
			writer.WriteLine($"typeof({className}),");
		}
		writer.Indent--;
		writer.WriteLine("}.ToImmutableArray();");
	}
}
