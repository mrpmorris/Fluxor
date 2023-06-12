using Fluxor.StoreBuilderSourceGenerator.Helpers;

namespace Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;

internal readonly record struct EffectMethodInfo
(
	string ClassNamespace,
	string ClassName,
	string MethodName,
	string ExplicitlyDeclaredActionClassFullName,
	string ActionClassFullName,
	bool IsStatic
)
{
	public string ClassFullName => NamespaceHelper.Combine(ClassNamespace, ClassName);
}
