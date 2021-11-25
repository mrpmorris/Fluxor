using System.Threading;

namespace Fluxor.UnitTests.StoreTests.UnhandledExceptionTests.SupportFiles
{
	public class ThrowAggregateExceptionAction
	{
		public ManualResetEvent TriggerHasFinished { get; } = new(initialState: false);
	}
}
