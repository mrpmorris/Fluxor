using System.Threading.Tasks;

// Each type makes AddFluxor throw; scanned only with ScanTypes
namespace Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.SupportFiles;

public class ReducerWithOneParameter
{
	[ReducerMethod]
	public static ScanState Reduce(ScanState state) => state;
}

public class ReducerWithActionTypeAndTwoParameters
{
	[ReducerMethod(typeof(ScanAction))]
	public static ScanState Reduce(ScanState state, ScanAction action) => state;
}

public class ReducerReturningAnotherType
{
	[ReducerMethod]
	public static object Reduce(ScanState state, ScanAction action) => state;
}

public class EffectWithOneParameter
{
	[EffectMethod]
	public static Task Handle(ScanAction action) => Task.CompletedTask;
}

public class EffectWithActionTypeAndTwoParameters
{
	[EffectMethod(typeof(ScanAction))]
	public static Task Handle(ScanAction action, IDispatcher dispatcher) => Task.CompletedTask;
}

public class EffectWithoutDispatcher
{
	[EffectMethod]
	public static Task Handle(ScanAction action, object dispatcher) => Task.CompletedTask;
}

public class EffectNotReturningTask
{
	[EffectMethod]
	public static ValueTask Handle(ScanAction action, IDispatcher dispatcher) => default;
}

[FeatureState]
public record StateWithoutConstructor(int Value);

[FeatureState(CreateInitialStateMethodName = nameof(Create))]
public record StateWithFactoryWithParameter(int Value)
{
	public static StateWithFactoryWithParameter Create(int value) => new(value);
}

[FeatureState(CreateInitialStateMethodName = nameof(Create))]
public record StateWithFactoryReturningObject(int Value)
{
	public static object Create() => new StateWithFactoryReturningObject(1);
}

[FeatureState(CreateInitialStateMethodName = "Missing")]
public record StateWithMissingFactory(int Value);
