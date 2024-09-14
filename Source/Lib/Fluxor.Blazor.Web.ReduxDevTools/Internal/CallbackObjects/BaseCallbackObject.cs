namespace Fluxor.Blazor.Web.ReduxDevTools.Internal.CallbackObjects;

public class BaseCallbackObject<TPayload>
	where TPayload : BasePayload
{
#pragma warning disable IDE1006 // Naming Styles
	public string type { get; set; }
	public TPayload payload { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

public class BaseCallbackObject : BaseCallbackObject<BasePayload> { }
