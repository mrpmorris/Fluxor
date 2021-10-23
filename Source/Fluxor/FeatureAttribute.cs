using System;

namespace Fluxor
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class FeatureAttribute : Attribute
	{
		public string Name { get; set; }
		public string CreateInitialStateMethodName { get; set; }
		public byte MaximumStateChangedNotificationsPerSecond { get; set; }
	}
}
