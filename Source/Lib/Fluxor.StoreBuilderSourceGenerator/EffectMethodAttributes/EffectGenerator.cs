﻿using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;

internal static class EffectGenerator
{
	public static Void Generate(SourceProductionContext productionContext, EffectMethodInfo effectMethodInfo)
	{
		string fileName = UniqueFilenameGenerator.Generate(
			type: UniqueFilenameGenerator.FileType.Effect,
			classNamespace: effectMethodInfo.ClassNamespace,
			className: effectMethodInfo.ClassName,
			uniqueMethodName: GetGeneratedClassName(effectMethodInfo));
		string sourceCode = GenerateSourceCode(effectMethodInfo);
		productionContext.AddSource(fileName, sourceCode);
		return Void.Value;
	}

	public static string GetGeneratedClassName(EffectMethodInfo effectMethodInfo) =>
		$"{effectMethodInfo.ClassName}_GeneratedFluxorEffect{effectMethodInfo.GetHashCode():X}".Replace('-', 'X');

	private static string GenerateSourceCode(EffectMethodInfo effectMethodInfo)
	{
		string generatedClassName = GetGeneratedClassName(effectMethodInfo);

		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		writer.WriteLine($"namespace {effectMethodInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, effectMethodInfo, generatedClassName);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClass(IndentedTextWriter writer, EffectMethodInfo effectMethodInfo, string generatedClassName)
	{
		writer.WriteLine($"internal sealed class {generatedClassName} : Fluxor.Effect<{effectMethodInfo.ActionClassFullName}>");
		using (writer.CodeBlock())
		{
			if (!effectMethodInfo.IsStatic)
				WriteConstructor(writer, effectMethodInfo, generatedClassName);

			OverrideHandleAsyncMethod(writer, effectMethodInfo);
		}
	}

	private static void WriteConstructor(IndentedTextWriter writer, EffectMethodInfo effectMethodInfo, string generatedClassName)
	{
		writer.WriteLine($"private readonly {effectMethodInfo.ClassFullName} Implementor;");
		writer.WriteLine();
		writer.WriteLine($"public {generatedClassName}({effectMethodInfo.ClassFullName} implementor)");
		using (writer.CodeBlock())
		{
			writer.WriteLine("Implementor = implementor;");
		}
		writer.WriteLine();
	}

	private static void OverrideHandleAsyncMethod(IndentedTextWriter writer, EffectMethodInfo effectMethodInfo)
	{
		string executingParty =
			effectMethodInfo.IsStatic
			? effectMethodInfo.ClassFullName
			: "Implementor";

		string actionArguments =
			effectMethodInfo.ExplicitlyDeclaredActionClassFullName is not null
			? "dispatcher"
			: "action, dispatcher";

		writer.WriteLine($"public override System.Threading.Tasks.Task HandleAsync({effectMethodInfo.ActionClassFullName} action, Fluxor.IDispatcher dispatcher) =>");
		writer.WriteIndentedLine($"{executingParty}.{effectMethodInfo.MethodName}({actionArguments});");
	}


}
