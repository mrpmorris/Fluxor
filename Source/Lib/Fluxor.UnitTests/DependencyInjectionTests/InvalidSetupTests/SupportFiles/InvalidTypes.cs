using System.Threading.Tasks;

// Every type in this namespace is invalid on purpose. The middleware below keeps assembly scans from
// other tests out of this namespace; each test scans the one type it needs with ScanTypes.
namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests.SupportFiles;

public class DoNotScanThisNamespace : Middleware
{
}

public class TestState
{
}

public class TestAction
{
}

public static class ReducerWithoutActionParameter
{
	[ReducerMethod]
	public static TestState Reduce(TestState state) => state;
}

public static class ReducerWithActionTypeAndActionParameter
{
	[ReducerMethod(typeof(TestAction))]
	public static TestState Reduce(TestState state, TestAction action) => state;
}

public static class ReducerReturningWrongType
{
	[ReducerMethod]
	public static int Reduce(TestState state, TestAction action) => 0;
}

public static class EffectWithoutDispatcherParameter
{
	[EffectMethod]
	public static Task Handle(TestAction action) => Task.CompletedTask;
}

public static class EffectWithActionTypeAndActionParameter
{
	[EffectMethod(typeof(TestAction))]
	public static Task Handle(TestAction action, IDispatcher dispatcher) => Task.CompletedTask;
}

public static class EffectWithoutDispatcherAsLastParameter
{
	[EffectMethod]
	public static Task Handle(TestAction action, string notADispatcher) => Task.CompletedTask;
}

public static class EffectReturningVoid
{
	[EffectMethod]
	public static void Handle(TestAction action, IDispatcher dispatcher) { }
}

[FeatureState]
public class StateWithoutParameterlessConstructor
{
	public StateWithoutParameterlessConstructor(int value) { }
}

[FeatureState(CreateInitialStateMethodName = "DoesNotExist")]
public class StateWithMissingFactoryMethod
{
}

[FeatureState(CreateInitialStateMethodName = nameof(Create))]
public class StateWithFactoryMethodWithParameter
{
	public static StateWithFactoryMethodWithParameter Create(int value) => new();
}

[FeatureState(CreateInitialStateMethodName = nameof(Create))]
public class StateWithFactoryMethodReturningWrongType
{
	public static TestState Create() => new();
}

[FeatureState(CreateInitialStateMethodName = nameof(Create))]
public class StateWithInstanceFactoryMethod
{
	public StateWithInstanceFactoryMethod Create() => new();
}

public class GenericType<T>
{
}

public class GenericType<T1, T2>
{
}
