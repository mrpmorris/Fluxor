﻿using Fluxor.Blazor.Web.ReduxDevTools.CallbackObjects;
using Fluxor.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	/// <summary>
	/// Middleware for interacting with the Redux Devtools extension for Chrome
	/// </summary>
	internal sealed class ReduxDevToolsMiddleware : WebMiddleware
	{
		private Task TailTask = Task.CompletedTask;
		private SpinLock SpinLock = new SpinLock();
		private int SequenceNumberOfCurrentState = 0;
		private int SequenceNumberOfLatestState = 0;
		private IDispatcher Dispatcher;
		private IStore Store;
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

		/// <see cref="IMiddleware.InitializeAsync(IDispatcher, IStore)"/>
		public async override Task InitializeAsync(IDispatcher dispatcher, IStore store)
		{
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			Store = store ?? throw new ArgumentNullException(nameof(store));
			await ReduxDevToolsInterop.InitializeAsync(GetState()).ConfigureAwait(false);
		}

		/// <see cref="IMiddleware.MayDispatchAction(object)"/>
		public override bool MayDispatchAction(object action) =>
			SequenceNumberOfCurrentState == SequenceNumberOfLatestState;

		/// <see cref="IMiddleware.AfterDispatch(object)"/>
		public override void AfterDispatch(object action)
		{
			SpinLock.ExecuteLocked(() =>
				{
					IDictionary<string, object> state = GetState();
					TailTask = TailTask
						.ContinueWith(_ => ReduxDevToolsInterop.DispatchAsync(action, state)).Unwrap();
				});

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
			// Wait for fire+forget state notifications to ReduxDevTools to dequeue
			await TailTask.ConfigureAwait(false);

			await ReduxDevToolsInterop.InitializeAsync(GetState()).ConfigureAwait(false);
			SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
		}

		private async Task OnJumpToState(JumpToStateCallback callbackInfo)
		{
			// Wait for fire+forget state notifications to ReduxDevTools to dequeue
			await TailTask.ConfigureAwait(false);

			SequenceNumberOfCurrentState = callbackInfo.payload.actionId;
			using (Store.BeginInternalMiddlewareChange())
			{
				var newFeatureStates = JsonConvert.DeserializeObject<Dictionary<string, object>>(callbackInfo.state);
				foreach (KeyValuePair<string, object> newFeatureState in newFeatureStates)
				{
					// Get the feature with the given name
					if (!Store.Features.TryGetValue(newFeatureState.Key, out IFeature feature))
						continue;

					object stronglyTypedFeatureState = JsonConvert.DeserializeObject(
						value: newFeatureState.Value.ToString(),
						type: feature.GetStateType());

					// Now set the feature's state to the deserialized object
					feature.RestoreState(stronglyTypedFeatureState);
				}
			}
		}
	}
}
