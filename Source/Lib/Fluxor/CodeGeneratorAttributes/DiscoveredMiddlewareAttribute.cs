using System;

namespace Fluxor.CodeGeneratorAttributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DiscoveredMiddlewareAttribute : Attribute
{
	public Type Middleware { get; }

	public DiscoveredMiddlewareAttribute(Type middleware)
	{
		Middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
	}
}
