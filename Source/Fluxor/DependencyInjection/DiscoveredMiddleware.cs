using System;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredMiddleware
	{
		public readonly Type ImplementingType;
		public readonly bool AutoLoaded;
		public readonly AssemblyScanSettings ScanSettings;

		public DiscoveredMiddleware(Type implementingType, bool autoLoaded)
		{
			ImplementingType = implementingType;
			AutoLoaded = autoLoaded;
			ScanSettings = new AssemblyScanSettings(ImplementingType.Assembly, ImplementingType.Namespace);
		}
	}
}
