namespace Fluxor.StoreBuilderSourceGeneratorTests;

//[FeatureState(CreateInitialStateMethodName = nameof(Create), MaximumStateChangedNotificationsPerSecond = 8, Name = nameof(TestState2) + "X")]
internal class TestState2
{
	public static TestState1 Create() => new TestState1();
}
