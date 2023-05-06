using Fluxor.StoreBuilderSourceGenerator.Helpers;

namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;

internal readonly record struct ReducerMethodInfo
(
	string ClassName,
	string ClassNamespace,
	string MethodName,
	string StateClassFullName,
	string ExplicitlyDeclaredActionClassFullName,
	string ActionClassFullName
)
{
	public string ClassFullName => NamespaceHelper.Combine(ClassNamespace, ClassName);
}
