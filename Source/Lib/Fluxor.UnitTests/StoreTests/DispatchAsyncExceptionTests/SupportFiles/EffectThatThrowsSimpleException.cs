using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.DispatchAsyncExceptionTests.SupportFiles;

public class EffectThatThrowsSimpleException : Effect<ThrowSimpleExceptionAction>
{
	public override async Task HandleAsync(ThrowSimpleExceptionAction action, IDispatcher dispatcher)
	{
		await Task.Delay(100); // Ensure it doesn't execute synchronously
		throw new InvalidOperationException("This is a simple exception");
	}
}
