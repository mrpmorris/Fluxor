using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;

internal static class EffectGenerator
{
	public static Void Generate(SourceProductionContext productionContext, EffectMethodInfo effectMethodInfo)
	{
		string fileName =
		$"{effectMethodInfo.ClassFullName}.{effectMethodInfo.MethodName}.Fluxor.Effect.{effectMethodInfo.GetHashCode()}.cs"
			.Replace("-", "X");

		string sourceCode = GenerateSourceCode(effectMethodInfo);
		productionContext.AddSource(fileName, sourceCode);
		return Void.Value;
	}

	private static string GenerateSourceCode(EffectMethodInfo effectMethodInfo)
	{
		string generatedClassName = $"{effectMethodInfo.ClassName}{effectMethodInfo.GetHashCode()}GeneratedFluxorEffect".Replace("-", "X");

		using var result = new StringWriter();
		using var writer = new IndentedTextWriter(result, tabString: "\t");

		WriteClassRegistration(writer, effectMethodInfo, generatedClassName);

		writer.WriteLine($"namespace {effectMethodInfo.ClassNamespace}");
		using (writer.CodeBlock())
		{
			WriteClass(writer, effectMethodInfo, generatedClassName);
		}

		writer.Flush();
		return result.ToString();
	}

	private static void WriteClassRegistration(IndentedTextWriter writer, EffectMethodInfo effectMethodInfo, string generatedClassName)
	{
		string implementingClassArgument =
			effectMethodInfo.IsStatic
			? ""
			: $", ImplementingClass = typeof({effectMethodInfo.ClassFullName})";

		string classFullName = NamespaceHelper.Combine(effectMethodInfo.ClassNamespace, generatedClassName);
		writer.WriteLine($"[assembly:Fluxor.CodeGeneratorAttributes.DiscoveredEffect(typeof({classFullName}){implementingClassArgument})]\r\n");
	}

	private static void WriteClass(IndentedTextWriter writer, EffectMethodInfo effectMethodInfo, string generatedClassName)
	{
		writer.WriteLine($"internal sealed class {generatedClassName} : Effect<{effectMethodInfo.ActionClassFullName}>");
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
