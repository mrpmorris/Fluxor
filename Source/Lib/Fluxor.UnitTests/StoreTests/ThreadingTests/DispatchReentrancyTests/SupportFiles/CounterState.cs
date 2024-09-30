namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchReentrancyTests.SupportFiles;

public class CounterState
{
	public readonly int Counter;

	public CounterState(int counter)
	{
		Counter = counter;
	}
}

