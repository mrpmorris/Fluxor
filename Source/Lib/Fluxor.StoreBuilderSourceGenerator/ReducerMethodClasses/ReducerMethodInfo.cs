namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodClasses;

internal readonly record struct ReducerMethodInfo
(
	string ClassFullName,
	string MethodName,
	string DeclaredActionClassFullName
);
