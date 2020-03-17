using Fluxor;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiddlewareSample.Client.Middlewares.Logging
{
	public class LoggingMiddleware : Middleware
	{
		public override Task InitializeAsync(IStore store)
		{
			Debug.WriteLine(nameof(InitializeAsync));
			return Task.CompletedTask;
		}

		public override void AfterInitializeAllMiddlewares()
		{
			Debug.WriteLine(nameof(AfterInitializeAllMiddlewares));
		}

		public override bool MayDispatchAction(object action)
		{
			Debug.WriteLine(nameof(MayDispatchAction) + ObjectInfo(action));
			return true;
		}

		public override void BeforeDispatch(object action)
		{
			Debug.WriteLine(nameof(BeforeDispatch) + ObjectInfo(action));
		}

		public override void AfterDispatch(object action)
		{
			Debug.WriteLine(nameof(AfterDispatch) + ObjectInfo(action));
		}

		private string ObjectInfo(object obj)
			=> ": " + obj.GetType().Name + " = " + JsonConvert.SerializeObject(obj);
	}
}
