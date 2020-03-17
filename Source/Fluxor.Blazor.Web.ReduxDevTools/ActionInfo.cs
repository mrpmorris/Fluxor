using System;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	public class ActionInfo
	{
#pragma warning disable IDE1006 // Naming Styles
		public string type { get; }
#pragma warning restore IDE1006 // Naming Styles
		public object Payload { get; }

		public ActionInfo(object action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			type = action.GetType().FullName;
			Payload = action;
		}
	}
}
