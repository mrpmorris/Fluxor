using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.LifecycleTests;

/// <summary>Records every lifecycle call it receives, in order, into a shared log.</summary>
public class RecordingMiddleware : Middleware
{
	private readonly string Name;
	private readonly List<string> Log;

	public int InitializeCount { get; private set; }
	public TaskCompletionSource<bool> InitializeGate { get; set; }

	public RecordingMiddleware(string name, List<string> log)
	{
		Name = name;
		Log = log;
	}

	public override async Task InitializeAsync(IDispatcher dispatcher, IStore store)
	{
		InitializeCount++;
		Write($"InitializeAsync");
		if (InitializeGate is not null)
			await InitializeGate.Task;
	}

	public override void AfterInitializeAllMiddlewares() => Write("AfterInitializeAllMiddlewares");
	public override void BeforeDispatch(object action) => Write($"BeforeDispatch {action.GetType().Name}");
	public override void AfterDispatch(object action) => Write($"AfterDispatch {action.GetType().Name}");

	private void Write(string entry)
	{
		lock (Log)
			Log.Add($"{Name}.{entry}");
	}
}
