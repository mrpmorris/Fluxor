namespace Fluxor.UnitTests.DependencyInjectionTests.IsolationTests.SupportFiles;

[FeatureState]
public class CounterState
{
	public int Counter { get; private set; }

	private CounterState() { }

	public CounterState(int counter)
	{
		Counter = counter;
	}
}
