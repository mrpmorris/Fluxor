using Fluxor.StoreBuilderSourceGenerator.Helpers;

namespace Fluxor.StoreBuilderSourceGenerator.FeatureStateClasses;

internal readonly record struct FeatureStateClassInfo
(
	string ClassName,
	string ClassNamespace,
	string CreateInitialStateMethodName,
	byte MaximumStateChangedNotificationsPerSecond,
	string StateName
)
{
	public string ClassFullName => NamespaceHelper.Combine(@namespace: ClassNamespace, className: ClassName);
}
