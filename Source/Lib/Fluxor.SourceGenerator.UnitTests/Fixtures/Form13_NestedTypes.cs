using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

// Every nested type has two attributed methods per kind, in an order that changes the result
public class Outer
{
	[FeatureState]
	public record NestedState(string Text)
	{
		public NestedState() : this("") { }
	}

	public record NestedAction;

	public class Inner
	{
		[ReducerMethod]
		public static NestedState Reduce(NestedState state, NestedAction action) => state with { Text = state.Text + "i;" };

		[ReducerMethod(typeof(NestedAction))]
		public NestedState ReduceAgain(NestedState state) => state with { Text = "(" + state.Text + ")" };

		public class Deeper
		{
			[ReducerMethod(typeof(NestedAction))]
			public NestedState Reduce(NestedState state) => state with { Text = state.Text + "d;" };

			[ReducerMethod]
			public static NestedState ReduceAgain(NestedState state, NestedAction action) => state with { Text = "<" + state.Text + ">" };

			[EffectMethod]
			public static Task HandleFirst(NestedAction action, IDispatcher dispatcher)
			{
				dispatcher.Dispatch(new Shared.Logged("deeper first"));
				return Task.CompletedTask;
			}

			[EffectMethod(typeof(NestedAction))]
			public Task HandleSecond(IDispatcher dispatcher)
			{
				dispatcher.Dispatch(new Shared.Logged("deeper second"));
				return Task.CompletedTask;
			}
		}
	}

	public class NestedEffect : Effect<NestedAction>
	{
		public override Task HandleAsync(NestedAction action, IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new Shared.Logged("nested effect"));
			return Task.CompletedTask;
		}
	}

	[ReducerMethod]
	public static NestedState ReduceOnOuter(NestedState state, NestedAction action) => state with { Text = state.Text + "o;" };

	[ReducerMethod(typeof(NestedAction))]
	public NestedState ReduceOnOuterAgain(NestedState state) => state with { Text = "{" + state.Text + "}" };
}
