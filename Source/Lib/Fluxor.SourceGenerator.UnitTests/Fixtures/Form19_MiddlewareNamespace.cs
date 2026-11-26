using Fluxor;
using System.Threading.Tasks;

namespace Fixture
{
	[FeatureState]
	public record TextState(string Text)
	{
		public TextState() : this("") { }
	}

	public record AppendAction;

	public class OutsideReducers
	{
		[ReducerMethod]
		public static TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "outside;" };
	}
}

namespace Fixture.WithMiddleware
{
	public class SomeMiddleware : Middleware { }

	public class ReducersNextToMiddleware
	{
		[ReducerMethod]
		public static TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "next to middleware;" };
	}

	public class EffectNextToMiddleware : Effect<AppendAction>
	{
		public override Task HandleAsync(AppendAction action, IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new Shared.Logged("next to middleware"));
			return Task.CompletedTask;
		}
	}

	[FeatureState]
	public record StateNextToMiddleware(int Value)
	{
		public StateNextToMiddleware() : this(9) { }
	}
}

namespace Fixture.WithMiddleware.Child
{
	public class ReducersBelowMiddleware
	{
		[ReducerMethod]
		public static TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "below middleware;" };
	}
}
