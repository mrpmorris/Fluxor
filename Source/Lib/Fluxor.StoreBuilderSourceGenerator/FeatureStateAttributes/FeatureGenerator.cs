using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;

internal static class FeatureGenerator
{
	public static Void Generate(SourceProductionContext productionContext, FeatureStateClassInfo featureStateClassInfo)
	{
		string sourceCode = GenerateSourceCode(featureStateClassInfo);
		productionContext.AddSource($"{featureStateClassInfo.ClassFullName}.Fluxor.Feature.cs", sourceCode);
		return Void.Value;
	}

	private static string GenerateSourceCode(FeatureStateClassInfo featureStateClassInfo)
	{
		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		writer.WriteLine("using Fluxor;\r\n");

		writer.WriteLine($"namespace {featureStateClassInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, featureStateClassInfo);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClass(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo)
	{
		writer.WriteLine($"internal class {featureStateClassInfo.ClassName}GeneratedFeature : Feature<{featureStateClassInfo.ClassName}>");
		using (writer.CodeBlock())
		{
			WriteConstructor(writer, featureStateClassInfo);
			OverrideGetName(writer, featureStateClassInfo);
			OverrideGetInitialState(writer, featureStateClassInfo);
		}
	}

	private static void WriteConstructor(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo)
	{
		writer.WriteLine($"public {featureStateClassInfo.ClassName}GeneratedFeature()");
		using (writer.CodeBlock())
		{
			writer.WriteLine($"MaximumStateChangedNotificationsPerSecond =  {featureStateClassInfo.MaximumStateChangedNotificationsPerSecond};");
		}
	}

	private static void OverrideGetName(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo)
	{
		string name = featureStateClassInfo.StateName ?? featureStateClassInfo.ClassName;
		writer.WriteLine($"public override string GetName() => \"{name}\";");
	}

	private static void OverrideGetInitialState(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo)
	{
		string creationCode =
			featureStateClassInfo.CreateInitialStateMethodName is null
			? $"new {featureStateClassInfo.ClassFullName}()"
			: $"{featureStateClassInfo.ClassFullName}.{featureStateClassInfo.CreateInitialStateMethodName}()";

		writer.WriteLine($"protected override {featureStateClassInfo.ClassFullName} GetInitialState() => {creationCode};");
	}


}
