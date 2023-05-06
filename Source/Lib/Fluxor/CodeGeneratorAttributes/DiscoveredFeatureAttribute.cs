using System;

namespace Fluxor.CodeGeneratorAttributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DiscoveredFeatureAttribute : Attribute
{
	public Type Feature { get; }

	public DiscoveredFeatureAttribute(Type feature)
	{
		Feature = feature ?? throw new ArgumentNullException(nameof(feature));
	}
}
