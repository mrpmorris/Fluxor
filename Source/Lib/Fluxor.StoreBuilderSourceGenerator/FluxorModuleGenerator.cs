using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.Helpers;
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
		string[] generatedEffectClassNames = discoveredClasses.EffectMethodInfos
			.Select(x => NamespaceHelper.Combine(x.ClassNamespace, EffectGenerator.GetGeneratedClassName(x)))
			.ToArray();
		
		string[] generatedFeatureClassNames = discoveredClasses.FeatureStateClassInfos
			.Select(x => NamespaceHelper.Combine(x.ClassNamespace, FeatureGenerator.GetGeneratedClassName(x)))
			.ToArray();

		string[] generatedReducerClassNames = discoveredClasses.ReducerMethodInfos
			.Select(x => NamespaceHelper.Combine(x.ClassNamespace, ReducerGenerator.GetGeneratedClassName(x)))
			.ToArray();

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

		writer.WriteLine("public partial class FluxorModule : Fluxor.IFluxorModule");
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
		writer.WriteLine($"public System.Collections.Generic.IEnumerable<System.Type> {propertyName} => _{propertyName};");
		writer.WriteLine($"private System.Collections.Generic.IEnumerable<System.Type> _{propertyName} = new System.Collections.Generic.List<System.Type>");
		writer.Indent++;
		writer.WriteLine("{");
		writer.Indent++;
		for(int i = 0; i < classes.Length; i++)
		{
			string className = classes[i];
			writer.WriteLine($"typeof({className}),");
		}
		writer.Indent--;
		writer.WriteLine("}");
		writer.WriteLine(".AsReadOnly();\r\n");
		writer.Indent--;
	}
}
