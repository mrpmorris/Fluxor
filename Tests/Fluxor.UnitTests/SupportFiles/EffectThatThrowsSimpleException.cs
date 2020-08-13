using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class EffectThatThrowsSimpleException : Effect<ThrowSimpleExceptionAction>
	{
		protected override async Task HandleAsync(ThrowSimpleExceptionAction action, IDispatcher dispatcher)
		{
			await Task.Delay(10).ConfigureAwait(false);
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
