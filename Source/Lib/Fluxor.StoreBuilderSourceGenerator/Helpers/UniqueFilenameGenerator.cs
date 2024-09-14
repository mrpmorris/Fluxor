using System.Runtime.CompilerServices;
using System;

internal static class UniqueFilenameGenerator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Generate(FileType type, string classNamespace, string className, string uniqueMethodName)
	{
		int hashCode = Math.Abs($"{type}/{classNamespace}/{className}/{uniqueMethodName}".GetHashCode());
		return $"Fluxor-{type}-{hashCode}";
	}

	public enum FileType
	{
		Reducer,
		Effect,
		Feature
	}
}
