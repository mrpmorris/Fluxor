using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.UnhandledExceptionTests.SupportFiles
{
	public class EffectThatThrowsAggregateException : Effect<ThrowAggregateExceptionAction>
	{
		protected override async Task HandleAsync(ThrowAggregateExceptionAction action, IDispatcher dispatcher)
		{
			var exception1 = new InvalidOperationException("First embedded exception");
			var exception2 = new InvalidCastException("Second embedded exception");
			var exception3 = new InvalidProgramException("Third embedded exception");

			await Task.Delay(100).ConfigureAwait(false);
			try
			{
				throw new AggregateException(
					exception1,
					new AggregateException(exception2, exception3));
			}
			finally
			{
				action.TriggerHasFinished.Set();
			}
		}
	}
}
