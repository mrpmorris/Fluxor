using System.Threading;

namespace Fluxor.UnitTests.SupportFiles
{
	public class ThrowAggregateExceptionAction
	{
		public ManualResetEvent TriggerHasFinished { get; } = new ManualResetEvent(false);
	}
}
