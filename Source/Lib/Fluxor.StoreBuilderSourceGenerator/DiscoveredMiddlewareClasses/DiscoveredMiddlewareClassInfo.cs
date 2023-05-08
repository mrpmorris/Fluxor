using Fluxor.StoreBuilderSourceGenerator.Helpers;

namespace Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;

internal readonly record struct DiscoveredMiddlewareClassInfo
(
	string ClassName,
	string ClassNamespace
)
{
	public static readonly DiscoveredMiddlewareClassInfo None = new();
	public string ClassFullName => NamespaceHelper.Combine(@namespace: ClassNamespace, className: ClassName);
}
