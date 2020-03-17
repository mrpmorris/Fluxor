using Fluxor.Blazor.Web.ReduxDevTools.CallbackObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	/// <summary>
	/// Middleware for interacting with the Redux Devtools extension for Chrome
	/// </summary>
	public sealed class ReduxDevToolsMiddleware : WebMiddleware
	{
		private int SequenceNumberOfCurrentState = 0;
		private int SequenceNumberOfLatestState = 0;
		private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

		/// <summary>
		/// Creates a new instance of the middleware
		/// </summary>
		public ReduxDevToolsMiddleware(ReduxDevToolsInterop reduxDevToolsInterop)
		{
			ReduxDevToolsInterop = reduxDevToolsInterop;
			ReduxDevToolsInterop.OnJumpToState = OnJumpToState;
			ReduxDevToolsInterop.OnCommit = OnCommit;
		}

		/// <see cref="IMiddleware.GetClientScripts"/>
		public override string GetClientScripts() => ReduxDevToolsInterop.GetClientScripts();

		/// <see cref="IMiddleware.InitializeAsync(IStore)"/>
		public async override Task InitializeAsync(IStore store)
		{
			await base.InitializeAsync(store);
			await ReduxDevToolsInterop.InitializeAsync(GetState());
		}

		/// <see cref="IMiddleware.MayDispatchAction(object)"/>
		public override bool MayDispatchAction(object action) =>
			SequenceNumberOfCurrentState == SequenceNumberOfLatestState;

		/// <see cref="IMiddleware.AfterDispatch(object)"/>
		public override void AfterDispatch(object action)
		{
			ReduxDevToolsInterop.Dispatch(action, GetState());

			// As actions can only be executed if not in a historical state (yes, "a" historical, pronounce your H!)
			// ensure the latest is incremented, and the current = latest
			SequenceNumberOfLatestState++;
			SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
		}

		private IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();
			foreach (IFeature feature in Store.Features.Values.OrderBy(x => x.GetName()))
				state[feature.GetName()] = feature.GetState();
			return state;
		}

		private async Task OnCommit()
		{
			await ReduxDevToolsInterop.InitializeAsync(GetState());
			SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
		}

		private Task OnJumpToState(JumpToStateCallback callbackInfo)
		{
			SequenceNumberOfCurrentState = callbackInfo.payload.actionId;
			using (Store.BeginInternalMiddlewareChange())
			{
				var newFeatureStates = JsonConvert.DeserializeObject<Dictionary<string, object>>(callbackInfo.state);
				foreach (KeyValuePair<string, object> newFeatureState in newFeatureStates)
				{
					// Get the feature with the given name
					if (!Store.Features.TryGetValue(newFeatureState.Key, out IFeature feature))
						continue;

					var serializedFeatureStateElement = JsonConvert.SerializeObject(newFeatureState.Value);
					object stronglyTypedFeatureState = JsonConvert.DeserializeObject(
						value: serializedFeatureStateElement.ToString(),
						type: feature.GetStateType());

					// Now set the feature's state to the deserialized object
					feature.RestoreState(stronglyTypedFeatureState);
				}
			}
			return Task.CompletedTask;
		}
	}
}
