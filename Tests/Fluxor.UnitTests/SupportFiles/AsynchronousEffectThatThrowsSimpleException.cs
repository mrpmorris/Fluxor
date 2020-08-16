using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class AsynchronousEffectThatThrowsSimpleException : Effect<ThrowSimpleExceptionAction>
	{
		public bool WasExecuted { get; set; }

		protected override async Task HandleAsync(ThrowSimpleExceptionAction action, IDispatcher dispatcher)
		{
			WasExecuted = true;

			await Task.Delay(100);
			try
			{
				throw new InvalidOperationException("This is a simple exception");
			}
			finally
			{
				action.TriggerHasFinished.Set();
			}
		}
	}
}
