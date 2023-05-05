namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;

internal readonly record struct ReducerMethodInfo
(
	string ClassFullName,
	string MethodName,
	string DeclaredActionClassFullName
);
