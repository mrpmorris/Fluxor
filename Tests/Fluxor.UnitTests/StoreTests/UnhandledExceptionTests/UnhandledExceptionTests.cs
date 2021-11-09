using Fluxor.UnitTests.StoreTests.UnhandledExceptionTests.SupportFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace Fluxor.UnitTests.StoreTests.UnhandledExceptionTests
{
	public class UnhandledExceptionTests
	{
		private Store Subject;

		public UnhandledExceptionTests()
		{
			Subject = new Store();
			Subject.AddEffect(new EffectThatThrowsSimpleException());
			Subject.AddEffect(new EffectThatThrowsAggregateException());
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public async Task WhenTriggerThrowsUnhandledException_ThenEventIsTriggered()
		{
			IEnumerable<Exception> exceptions = await SendAction(new ThrowSimpleExceptionAction());
			Assert.Single(exceptions);
			Assert.IsType<InvalidOperationException>(exceptions.First());
		}

		[Fact]
		public async Task WhenTriggerThrowsUnhandledAggregateException_ThenEventIsTriggeredForEachEvent()
		{
			Type[] exceptionTypes = 
				(await SendAction(new ThrowAggregateExceptionAction()))
				.Select(x => x.GetType())
				.ToArray();

			Assert.Equal(3, exceptionTypes.Length);
			Assert.Contains(typeof(InvalidOperationException), exceptionTypes);
			Assert.Contains(typeof(InvalidCastException), exceptionTypes);
			Assert.Contains(typeof(InvalidProgramException), exceptionTypes);
		}

		private async Task<IEnumerable<Exception>> SendAction(object action)
		{
			var result = new List<Exception>();
			var resetEvent = new ManualResetEvent(false);

			Subject.UnhandledException += (sender, e) =>
			{
				result.Add(e.Exception);
				resetEvent.Set();
			};

			Task effectTask = Task.Run(() =>
			{
				Subject.Dispatch(action);
				// Wait for Effect to say it is ready, 1 second timeout
				resetEvent.WaitOne(1000);
			});

			await effectTask.ConfigureAwait(false);
			return result;
		}
	}
}
