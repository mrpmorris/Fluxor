using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchReentrancyTests.SupportFiles
{
	public class AutoIncrementMiddleware : Middleware
	{
		private IDispatcher Dispatcher;

		public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
		{
			Dispatcher = dispatcher;
			return base.InitializeAsync(dispatcher, store);
		}

		public override void AfterDispatch(object action)
		{
			if (action is StoreInitializedAction)
			{

			}
		}
	}
}
