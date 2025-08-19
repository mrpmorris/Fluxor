using Fluxor.Blazor.Web.ReduxDevTools.Internal;
using Fluxor.Blazor.Web.ReduxDevTools.Internal.CallbackObjects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.ReduxDevTools;

/// <summary>
/// Middleware for interacting with the Redux Devtools extension for Chrome
/// </summary>
public sealed class ReduxDevToolsMiddleware : WebMiddleware
{
	private readonly object SyncRoot = new();
	private Task TailTask = Task.CompletedTask;
	private int SequenceNumberOfCurrentState = 0;
	private int SequenceNumberOfLatestState = 0;
	private readonly ReduxDevToolsMiddlewareOptions Options;
	private IStore Store;
	private readonly IReduxDevToolsInterop ToolsInterop;

	/// <summary>
	/// Creates a new instance of the middleware
	/// </summary>
	public ReduxDevToolsMiddleware(
		IReduxDevToolsInterop reduxDevToolsInterop,
		ReduxDevToolsMiddlewareOptions options)
	{
		Options = options;
		ToolsInterop = reduxDevToolsInterop;
		ToolsInterop.OnJumpToState = OnJumpToState;
		ToolsInterop.OnCommit = OnCommit;
	}

	/// <see cref="IMiddleware.GetClientScripts"/>
	public override string GetClientScripts() => ReduxDevToolsInterop.GetClientScripts(Options);

	/// <see cref="IMiddleware.InitializeAsync(IStore)"/>
	public async override Task InitializeAsync(IDispatcher dispatcher, IStore store)
	{
		Store = store;
		await ToolsInterop.InitializeAsync(GetState());
	}

	/// <see cref="IMiddleware.MayDispatchAction(object)"/>
	public override bool MayDispatchAction(object action) =>
		SequenceNumberOfCurrentState == SequenceNumberOfLatestState;

	/// <see cref="IMiddleware.AfterDispatch(object)"/>
	public override void AfterDispatch(object action)
	{
		if (Options.ActionFilters.Length > 0 && !Options.ActionFilters.All(filter => filter(action)))
			return;

		string stackTrace = null;
		int maxItems = Options.StackTraceLimit == 0 ? int.MaxValue : Options.StackTraceLimit;
		if (Options.StackTraceEnabled)
			stackTrace =
				string.Join("\r\n",
					new StackTrace(fNeedFileInfo: true)
						.GetFrames()
						.Select(x => new { StackFrame = x, Method = x.GetMethod() })
						.Where(x => x.Method?.DeclaringType is not null)
						.Select(x => $"at {x.Method.DeclaringType.FullName}.{x.Method.Name} ({x.StackFrame.GetFileName()}:{x.StackFrame.GetFileLineNumber()}:{x.StackFrame.GetFileColumnNumber()})")
						.Where(x => Options.StackTraceFilterRegex?.IsMatch(x) != false)
						.Take(maxItems)
				);
		lock (SyncRoot)
		{
			IDictionary<string, object> state = GetState();
			TailTask = TailTask
				.ContinueWith(_ => ToolsInterop.DispatchAsync(action, state, stackTrace)).Unwrap();
		}

		// As actions can only be executed if not in a historical state (yes, "a" historical, pronounce your H!)
		// ensure the latest is incremented, and the current = latest
		SequenceNumberOfLatestState++;
		SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
	}

	private IDictionary<string, object> GetState()
	{
		var state = new Dictionary<string, object>();
		var serializableFeatures = Store.Features.Values.Where(x => x.DebuggerBrowsable);
		foreach (IFeature feature in serializableFeatures.OrderBy(x => x.GetName()))
			state[feature.GetName()] = feature.GetState();
		return state;
	}

	private async Task OnCommit()
	{
		// Wait for fire+forget state notifications to ReduxDevTools to dequeue
		await TailTask.ConfigureAwait(false);

		await ToolsInterop.InitializeAsync(GetState());
		SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
	}

	private async Task OnJumpToState(JumpToStateCallback callbackInfo)
	{
		// Wait for fire+forget state notifications to ReduxDevTools to dequeue
		await TailTask.ConfigureAwait(false);

		SequenceNumberOfCurrentState = callbackInfo.payload.actionId;
		using (Store.BeginInternalMiddlewareChange())
		{
			var newFeatureStates = JsonSerializer.Deserialize<Dictionary<string, object>>(
				json: callbackInfo.state,
				options: Options.JsonSerializerOptions);

			foreach (KeyValuePair<string, object> newFeatureState in newFeatureStates)
			{
				// Get the feature with the given name
				if (!Store.Features.TryGetValue(newFeatureState.Key, out IFeature feature))
					continue;

				object stronglyTypedFeatureState = JsonSerializer
					.Deserialize(
						json: newFeatureState.Value.ToString(),
						returnType: feature.GetStateType(),
						options: Options.JsonSerializerOptions);

				// Now set the feature's state to the deserialized object
				feature.RestoreState(stronglyTypedFeatureState);
			}
		}
	}
}
