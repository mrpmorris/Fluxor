namespace Fluxor.Blazor.Web
{
	public abstract class WebMiddleware : Middleware, IWebMiddleware
	{
		public abstract string GetClientScripts();
	}
}
