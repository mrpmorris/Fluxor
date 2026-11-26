using System.ComponentModel;
using System.Threading.Tasks;

// Scanned only with ScanTypes: the middleware keeps whole-assembly scans of the test assembly out of this namespace
namespace Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.SupportFiles;

public class DoNotScanThisNamespace : Middleware
{
}

[FeatureState]
public record ScanState(string Text)
{
	private ScanState() : this("") { }
}

public record ScanAction;
public record EffectRan(string Name);

public class EffectLogReducer
{
	[ReducerMethod]
	public static ScanState Reduce(ScanState state, EffectRan action) => state with { Text = state.Text + "[" + action.Name + "]" };
}

/// <summary>Every shape of reducer method the scan binds, on one host.</summary>
public class AllReducerShapes
{
	[ReducerMethod]
	public static ScanState StaticWithAction(ScanState state, ScanAction action) => state with { Text = state.Text + "sa;" };

	[ReducerMethod(typeof(ScanAction))]
	public static ScanState StaticWithoutAction(ScanState state) => state with { Text = state.Text + "s;" };

	[ReducerMethod]
	public ScanState InstanceWithAction(ScanState state, ScanAction action) => state with { Text = state.Text + "ia;" };

	[ReducerMethod(typeof(ScanAction))]
	public ScanState InstanceWithoutAction(ScanState state) => state with { Text = state.Text + "i;" };

	[Description("Another attribute does not hide the reducer")]
	[ReducerMethod]
	public static ScanState WithAnotherAttribute(ScanState state, ScanAction action) => state with { Text = state.Text + "x;" };
}

/// <summary>Every shape of effect method the scan binds, on one host.</summary>
public class AllEffectShapes
{
	[EffectMethod]
	public static Task StaticWithAction(ScanAction action, IDispatcher dispatcher) => Ran(dispatcher, "sa");

	[EffectMethod(typeof(ScanAction))]
	public static Task StaticWithoutAction(IDispatcher dispatcher) => Ran(dispatcher, "s");

	[EffectMethod]
	public Task InstanceWithAction(ScanAction action, IDispatcher dispatcher) => Ran(dispatcher, "ia");

	[EffectMethod(typeof(ScanAction))]
	public Task InstanceWithoutAction(IDispatcher dispatcher) => Ran(dispatcher, "i");

	private static Task Ran(IDispatcher dispatcher, string name)
	{
		dispatcher.Dispatch(new EffectRan(name));
		return Task.CompletedTask;
	}
}

public abstract class AttributedVirtualMethods
{
	[ReducerMethod]
	public virtual ScanState Reduce(ScanState state, ScanAction action) => state with { Text = state.Text + "base;" };

	[EffectMethod]
	public virtual Task Handle(ScanAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectRan("base"));
		return Task.CompletedTask;
	}
}

/// <summary>The attributes are on the overridden methods only, so nothing is registered.</summary>
public class OverridesWithoutAttributes : AttributedVirtualMethods
{
	public override ScanState Reduce(ScanState state, ScanAction action) => state with { Text = state.Text + "override;" };

	public override Task Handle(ScanAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectRan("override"));
		return Task.CompletedTask;
	}
}
