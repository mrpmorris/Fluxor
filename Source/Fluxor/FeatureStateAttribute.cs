using System;

namespace Fluxor
{
	[AttributeUsage(
		 AttributeTargets.Class | AttributeTargets.Struct,
		AllowMultiple = false, Inherited = false)]
	public sealed class FeatureStateAttribute : Attribute
	{
		public string Name { get; set; }
		public string CreateInitialStateMethodName { get; set; }
		public byte MaximumStateChangedNotificationsPerSecond { get; set; }
	}
}
