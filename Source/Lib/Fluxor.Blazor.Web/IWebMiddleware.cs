namespace Fluxor.Blazor.Web
{
	public interface IWebMiddleware : IMiddleware
	{
		/// <summary>
		/// If the Middleware requires scripts inside the browser in order to function
		/// then those scripts should be returned from this method
		/// </summary>
		/// <returns>Any required JavaScript, or null</returns>
		string GetClientScripts();
	}
}
