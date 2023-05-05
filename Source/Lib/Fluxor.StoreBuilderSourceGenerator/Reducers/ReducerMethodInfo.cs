namespace Fluxor.StoreBuilderSourceGenerator.Reducers;

internal readonly record struct ReducerMethodInfo
(
	string ClassFullName,
	string MethodName,
	string DeclaredActionClassFullName
);
