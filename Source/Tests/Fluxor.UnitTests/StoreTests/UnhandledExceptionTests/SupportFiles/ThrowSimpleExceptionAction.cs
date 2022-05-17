using System.Threading;

namespace Fluxor.UnitTests.StoreTests.UnhandledExceptionTests.SupportFiles
{
	public class ThrowSimpleExceptionAction
	{
		public ManualResetEvent TriggerHasFinished { get; } = new(initialState: false);
	}
}
