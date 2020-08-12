using System.Threading;

namespace Fluxor.UnitTests.SupportFiles
{
	public class ThrowSimpleExceptionAction
	{
		public ManualResetEvent TriggerHasFinished { get; } = new ManualResetEvent(false);
	}
}
