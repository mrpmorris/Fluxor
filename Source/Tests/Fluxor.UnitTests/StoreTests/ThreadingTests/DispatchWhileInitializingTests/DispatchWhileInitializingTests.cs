using Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchWhileInitializingTests.SupportFiles;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchWhileInitializingTests
{
	public class DispatchWhileInitializingTests
	{
		private readonly IDispatcher Dispatcher;
		private readonly IStore Subject;
		private readonly IFeature<CounterState> Feature;

		[Fact]
		public async Task WhenInitializeTakesAWhile_AndTriggersEffectThatDispatchesAnAction_AndAnotherThreadDispatchesAnAction_ThenThereShouldBeNoDeadlock()
		{
			Thread initialThread = Thread.CurrentThread; 

			var timeout = Task.Delay(1000_000);
			var initialize = Task.Run(async () => await Subject.InitializeAsync());
			var dispatch = Task.Run(async () =>
			{
				await Task.Delay(50);
				Dispatcher.Dispatch(new IncrementCounterAction());
			});

			await Task.WhenAny(timeout, Task.WhenAll(initialize, dispatch));
			Assert.False(timeout.IsCompleted, "Time out due to deadlock");
		}

		public DispatchWhileInitializingTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);

			Feature = new CounterFeature();
			Subject.AddFeature(Feature);
			Subject.AddEffect(new EffectThatEmitsActions(new[] { new TestAction() }));
		}
	}
}
