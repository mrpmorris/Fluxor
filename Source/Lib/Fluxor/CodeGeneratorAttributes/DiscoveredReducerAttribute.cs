using System;

namespace Fluxor.CodeGeneratorAttributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DiscoveredReducerAttribute : Attribute
{
	public Type Reducer { get; }

	public DiscoveredReducerAttribute(Type reducer)
	{
		Reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
	}
}
