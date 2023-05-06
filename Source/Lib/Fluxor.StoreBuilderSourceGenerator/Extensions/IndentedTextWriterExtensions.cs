using Fluxor.StoreBuilderSourceGenerator.Helpers;
using System;
using System.CodeDom.Compiler;

namespace Fluxor.StoreBuilderSourceGenerator.Extensions;

internal static class IndentedTextWriterExtensions
{
	public static IDisposable CodeBlock(this IndentedTextWriter instance)
	{
		instance.WriteLine("{");
		instance.Indent++;
		return new DisposableAction(() =>
		{
			instance.Indent--;
			instance.WriteLine("}");
		});
	}
}
