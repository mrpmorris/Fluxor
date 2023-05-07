using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;

internal static class ReducerGenerator
{
	public static Void Generate(SourceProductionContext productionContext, ReducerMethodInfo reducerMethodInfo)
	{
		string fileName =
			$"{reducerMethodInfo.ClassFullName}.{reducerMethodInfo.MethodName}.Fluxor.Reducer.{reducerMethodInfo.GetHashCode()}.cs"
			.Replace("-", "X");

		string sourceCode = GenerateSourceCode(reducerMethodInfo);
		productionContext.AddSource(fileName, sourceCode);
		return Void.Value;
	}

	private static string GenerateSourceCode(ReducerMethodInfo reducerMethodInfo)
	{
		string generatedClassName = $"{reducerMethodInfo.ClassName}{reducerMethodInfo.GetHashCode()}GeneratedFluxorReducer".Replace("-", "X");

		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		WriteClassRegistration(writer, reducerMethodInfo, generatedClassName);

		writer.WriteLine($"namespace {reducerMethodInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, reducerMethodInfo, generatedClassName);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClassRegistration(IndentedTextWriter writer, ReducerMethodInfo reducerMethodInfo, string generatedClassName)
	{
		string classFullName = NamespaceHelper.Combine(reducerMethodInfo.ClassNamespace, generatedClassName);
		writer.WriteLine($"[assembly:Fluxor.CodeGeneratorAttributes.DiscoveredReducer(typeof({classFullName}))]\r\n");
	}

	private static void WriteClass(IndentedTextWriter writer, ReducerMethodInfo reducerMethodInfo, string generatedClassName)
	{
		writer.WriteLine($"internal sealed class {generatedClassName} : Reducer<{reducerMethodInfo.StateClassFullName}, {reducerMethodInfo.ActionClassFullName}>");
		using (writer.CodeBlock())
		{
			OverrideReduce(writer, reducerMethodInfo);
		}
	}

	private static void OverrideReduce(IndentedTextWriter writer, ReducerMethodInfo reducerMethodInfo)
	{
		string arguments =
			reducerMethodInfo.ExplicitlyDeclaredActionClassFullName is not null
			? $"state"
			: $"state, action";

		writer.WriteLine($"public override {reducerMethodInfo.StateClassFullName} Reduce(");
		writer.WriteIndentedLine($"{reducerMethodInfo.StateClassFullName} state,");
		writer.WriteIndentedLine($"{reducerMethodInfo.ActionClassFullName} action)");
		writer.WriteLine("=>");
		writer.WriteIndentedLine($"{reducerMethodInfo.ClassFullName}.{reducerMethodInfo.MethodName}({arguments});");
	}
}
