using System;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	internal class ActionInfo
	{
#pragma warning disable IDE1006 // Naming Styles
		public string type { get; }
#pragma warning restore IDE1006 // Naming Styles
		public object Payload { get; }

		public ActionInfo(object action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			type = $"{action.GetType().Name} {action.GetType().Namespace}";
			Payload = action;
		}
	}
}
