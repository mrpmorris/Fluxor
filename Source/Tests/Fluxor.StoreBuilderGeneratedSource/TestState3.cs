namespace Fluxor.StoreBuilderGeneratedSource
{
	internal class TestState3
	{
	}

	internal readonly record struct TestState3Action1();

	internal class TestState3Feature : Feature<TestState3>
	{
		public override string GetName() => "TestState3";
		protected override TestState3 GetInitialState() => new TestState3();
	}

	internal class TestState3Action1Reducer : Reducer<TestState3, TestState3Action1>
	{
		public override TestState3 Reduce(TestState3 state, TestState3Action1 action) => state;
	}

	internal class TestState3Action1Effect : Effect<TestState3Action1>
	{
		public override Task HandleAsync(TestState3Action1 action, IDispatcher dispatcher) => Task.CompletedTask;
	}

}
