using System;

namespace Fluxor.CodeGeneratorAttributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DiscoveredEffectAttribute : Attribute
{
	public Type Effect { get; }
	public Type ImplementingClass { get; set; }

	public DiscoveredEffectAttribute(Type effect)
	{
		Effect = effect ?? throw new ArgumentNullException(nameof(effect));
	}
}
