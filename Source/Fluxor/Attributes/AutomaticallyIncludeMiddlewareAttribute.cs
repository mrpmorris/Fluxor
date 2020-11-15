using System;

namespace Fluxor
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class AutomaticallyIncludeMiddlewareAttribute : Attribute
	{
	}
}
