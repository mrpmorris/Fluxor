namespace Fluxor.StoreBuilderSourceGeneratorTests;

[FeatureState(CreateInitialStateMethodName = nameof(Create), MaximumStateChangedNotificationsPerSecond = 8, Name = nameof(TestState1))]
internal class TestState1
{
	public static TestState1 Create() => new TestState1();

	public int X = 43;
}

internal readonly record struct TestState1Action1();

internal class State1Reducers
{
	[ReducerMethod(typeof(TestState1Action1))]
	public static TestState1 ReduceTestState1Action1(TestState1 state) => state;

	public static string SayHello() => "Helpppplo";
}



