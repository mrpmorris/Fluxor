using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;

internal static class FeatureGenerator
{
	public static Void Generate(SourceProductionContext productionContext, FeatureStateClassInfo featureStateClassInfo)
	{
		string filename = FilenameGenerator.Generate(featureStateClassInfo.ClassNamespace, featureStateClassInfo.ClassName);
		string sourceCode = GenerateSourceCode(featureStateClassInfo);
		productionContext.AddSource(filename, sourceCode);
		return Void.Value;
	}

	public static string GetGeneratedClassName(FeatureStateClassInfo featureStateClassInfo) =>
		$"{featureStateClassInfo.ClassName}GeneratedFluxorFeature";

	private static string GenerateSourceCode(FeatureStateClassInfo featureStateClassInfo)
	{
		string generatedClassName = GetGeneratedClassName(featureStateClassInfo);

		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		writer.WriteLine($"namespace {featureStateClassInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, featureStateClassInfo, generatedClassName);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClass(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo, string generatedClassName)
	{
		writer.WriteLine($"internal sealed class {generatedClassName} : Fluxor.Feature<{featureStateClassInfo.ClassName}>");
		using (writer.CodeBlock())
		{
			OverrideGetName(writer, featureStateClassInfo);
			OverrideGetInitialState(writer, featureStateClassInfo);
			WriteConstructor(writer, featureStateClassInfo, generatedClassName);
		}
	}

	private static void WriteConstructor(IndentedTextWriter writer, FeatureStateClassInfo featureStateClassInfo, string generatedClassName)
	{
		writer.WriteLine();
		writer.WriteLine($"public {generatedClassName}()");
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
			? $"System.Activator.CreateInstance<{featureStateClassInfo.ClassFullName}>()"
			: $"{featureStateClassInfo.ClassFullName}.{featureStateClassInfo.CreateInitialStateMethodName}()";

		writer.WriteLine($"protected override {featureStateClassInfo.ClassFullName} GetInitialState() =>");
		writer.WriteIndentedLine($"{creationCode};\r\n");
	}
}
