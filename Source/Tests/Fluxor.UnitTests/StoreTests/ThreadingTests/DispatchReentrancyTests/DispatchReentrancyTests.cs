using Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchReentrancyTests.SupportFiles;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchReentrancyTests
{
	public class DispatchReentrancyTests
	{
		private readonly IDispatcher Dispatcher;
		private readonly IStore Subject;
		private readonly IFeature<CounterState> Feature;

		[Fact]
		public async Task WhenObserverSubscribesToAnAction_AndDispatchesAnActionFromANewThread_ThenThereShouldBeNoDeadlock()
		{
			Thread initialThread = Thread.CurrentThread;
			Subject.SubscribeToAction<StoreInitializedAction>(this, _ =>
			{
				var thread = new Thread(() =>
				{
					Thread.Sleep(50);
					while (Thread.CurrentThread == initialThread)
						Thread.Sleep(0);

					Dispatcher.Dispatch(new IncrementCounterAction());
				});
				thread.Start();
				thread.Join();
			});

			var timeout = Task.Delay(1000);
			var initialize = Task.Run(async () =>
			{
				await Task.Yield();
				await Subject.InitializeAsync();
			});
			await Task.WhenAny(timeout, initialize);
			Assert.False(timeout.IsCompleted, "Time out due to deadlock");
		}

		public DispatchReentrancyTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);

			Feature = new CounterFeature();
			Subject.AddFeature(Feature);
		}

	}
}
