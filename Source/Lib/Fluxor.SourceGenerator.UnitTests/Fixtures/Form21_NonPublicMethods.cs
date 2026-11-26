using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

[FeatureState]
public record VisibilityState(string Text)
{
	public VisibilityState() : this("") { }
}

public record VisibilityAction;

// The scan only looks at public methods: attributed methods of any other accessibility are not registered
public class MixedVisibilityReducers
{
	[ReducerMethod]
	public static VisibilityState ReducePublic(VisibilityState state, VisibilityAction action) => state with { Text = state.Text + "public;" };

	[ReducerMethod]
	internal static VisibilityState ReduceInternal(VisibilityState state, VisibilityAction action) => state with { Text = state.Text + "internal;" };

	[ReducerMethod]
	private static VisibilityState ReducePrivate(VisibilityState state, VisibilityAction action) => state with { Text = state.Text + "private;" };

	[ReducerMethod(typeof(VisibilityAction))]
	protected VisibilityState ReduceProtected(VisibilityState state) => state with { Text = state.Text + "protected;" };

	[ReducerMethod(typeof(VisibilityAction))]
	protected internal VisibilityState ReduceProtectedInternal(VisibilityState state) => state with { Text = state.Text + "protected internal;" };

	[EffectMethod]
	private static Task HandlePrivate(VisibilityAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("private effect"));
		return Task.CompletedTask;
	}

	[EffectMethod(typeof(VisibilityAction))]
	internal Task HandleInternal(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("internal effect"));
		return Task.CompletedTask;
	}
}
