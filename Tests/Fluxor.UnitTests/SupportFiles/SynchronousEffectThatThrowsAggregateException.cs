using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class SynchronousEffectThatThrowsAggregateException : Effect<ThrowAggregateExceptionAction>
	{
		public bool WasExecuted { get; set; }

		protected override Task HandleAsync(ThrowAggregateExceptionAction action, IDispatcher dispatcher)
		{
			WasExecuted = true;

			var exception1 = new InvalidOperationException("First embedded exception");
			var exception2 = new InvalidCastException("Second embedded exception");
			var exception3 = new InvalidProgramException("Third embedded exception");

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
