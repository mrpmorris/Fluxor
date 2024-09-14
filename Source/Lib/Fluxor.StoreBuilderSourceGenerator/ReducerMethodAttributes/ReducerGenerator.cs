using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;

internal static class ReducerGenerator
{
	public static Void Generate(SourceProductionContext productionContext, ReducerMethodInfo reducerMethodInfo)
	{
		string fileName = UniqueFilenameGenerator.Generate(
			type: UniqueFilenameGenerator.FileType.Reducer,
			classNamespace: reducerMethodInfo.ClassNamespace,
			className: reducerMethodInfo.ClassName, uniqueMethodName:
			GetGeneratedClassName(reducerMethodInfo));

		string sourceCode = GenerateSourceCode(reducerMethodInfo);
		productionContext.AddSource(fileName, sourceCode);
		return Void.Value;
	}

	public static string GetGeneratedClassName(ReducerMethodInfo reducerMethodInfo) =>
		$"{reducerMethodInfo.ClassName}_GeneratedFluxorReducer{reducerMethodInfo.GetHashCode():X}".Replace('-', 'X');

	private static string GenerateSourceCode(ReducerMethodInfo reducerMethodInfo)
	{
		string generatedClassName = GetGeneratedClassName(reducerMethodInfo);

		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		writer.WriteLine($"namespace {reducerMethodInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, reducerMethodInfo, generatedClassName);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClass(IndentedTextWriter writer, ReducerMethodInfo reducerMethodInfo, string generatedClassName)
	{
		writer.WriteLine($"internal sealed class {generatedClassName} : Fluxor.Reducer<{reducerMethodInfo.StateClassFullName}, {reducerMethodInfo.ActionClassFullName}>");
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
