using System.Collections.Generic;
using System.Threading.Tasks;

// Scanned explicitly with ScanTypes; the middleware keeps other tests' assembly scans out of this namespace.
namespace Fluxor.UnitTests.DependencyInjectionTests.InstanceMethodTests.SupportFiles;

public class DoNotScanThisNamespace : Middleware
{
}

[FeatureState]
public record CounterState(int Count)
{
	private CounterState() : this(0) { }
}

public record AddAction(int Amount);
public record ResetAction;

/// <summary>Non-static reducers that depend on instance members, so they only work with a host instance.</summary>
public class InstanceReducers
{
	private readonly int Multiplier = 10;
	private readonly int ResetValue = -1;

	[ReducerMethod]
	public CounterState Add(CounterState state, AddAction action) => state with { Count = state.Count + action.Amount * Multiplier };

	[ReducerMethod(typeof(ResetAction))]
	public CounterState Reset(CounterState state) => state with { Count = ResetValue };
}

public class EffectLog
{
	public List<string> Entries { get; } = new();
}

/// <summary>Non-static effects whose host is created by dependency injection.</summary>
public class InstanceEffects
{
	private readonly EffectLog Log;

	public InstanceEffects(EffectLog log)
	{
		Log = log;
	}

	[EffectMethod]
	public Task OnAdd(AddAction action, IDispatcher dispatcher)
	{
		Log.Entries.Add($"OnAdd {action.Amount}");
		return Task.CompletedTask;
	}

	[EffectMethod(typeof(ResetAction))]
	public Task OnReset(IDispatcher dispatcher)
	{
		Log.Entries.Add("OnReset");
		return Task.CompletedTask;
	}
}
