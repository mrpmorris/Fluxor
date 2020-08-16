using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class SynchronousEffectThatThrowsSimpleException : Effect<ThrowSimpleExceptionAction>
	{
		public bool WasExecuted { get; set; }

		protected override Task HandleAsync(ThrowSimpleExceptionAction action, IDispatcher dispatcher)
		{
			WasExecuted = true;

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
