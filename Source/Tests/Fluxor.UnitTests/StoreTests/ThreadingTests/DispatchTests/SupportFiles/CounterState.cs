namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchTests.SupportFiles;

public class CounterState
{
	public readonly int Counter;

	private CounterState() { }

	public CounterState(int counter)
	{
		Counter = counter;
	}
}
