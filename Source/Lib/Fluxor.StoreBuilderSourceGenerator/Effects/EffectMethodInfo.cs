namespace Fluxor.StoreBuilderSourceGenerator.Effects;

internal readonly record struct EffectMethodInfo
(
	string ClassFullName,
	string MethodName,
	string DeclaredActionClassFullName,
	bool IsStatic
);
