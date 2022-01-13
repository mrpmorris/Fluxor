using Fluxor.Blazor.Web.ReduxDevTools.CallbackObjects;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	/// <summary>
	/// Interop for dev tools
	/// </summary>
	internal sealed class ReduxDevToolsInterop : IDisposable
	{
		public const string DevToolsCallbackId = "DevToolsCallback";
		public bool DevToolsBrowserPluginDetected { get; private set; }
		public Func<JumpToStateCallback, Task> OnJumpToState;
		public Func<Task> OnCommit;

		private const string FluxorDevToolsId = "__FluxorDevTools__";
		private const string FromJsDevToolsDetectedActionTypeName = "detected";
		private const string ToJsDispatchMethodName = "dispatch";
		private const string ToJsInitMethodName = "init";
		private bool Disposed;
		private bool IsInitializing;
		private readonly IJSRuntime JSRuntime;
		private readonly IJsonSerialization JsonSerialization;
		private readonly DotNetObjectReference<ReduxDevToolsInterop> DotNetRef;

		/// <summary>
		/// Creates an instance of the dev tools interop
		/// </summary>
		/// <param name="jsRuntime"></param>
		public ReduxDevToolsInterop(
			IJSRuntime jsRuntime,
			IJsonSerialization jsonSerialization = null)
		{
			JSRuntime = jsRuntime;
			JsonSerialization = jsonSerialization ?? new Serialization.NewtonsoftJsonAdapter();
			DotNetRef = DotNetObjectReference.Create(this);
		}

		internal async ValueTask InitializeAsync(IDictionary<string, object> state)
		{
			IsInitializing = true;
			try
			{
				await InvokeFluxorDevToolsMethodAsync<object>(ToJsInitMethodName, DotNetRef, state);
			}
			finally
			{
				IsInitializing = false;
			}
		}

		internal async Task<object> DispatchAsync(object action, IDictionary<string, object> state) =>
			await InvokeFluxorDevToolsMethodAsync<object>(ToJsDispatchMethodName, new ActionInfo(action), state)
			 .ConfigureAwait(false);

		/// <summary>
		/// Called back from ReduxDevTools
		/// </summary>
		/// <param name="messageAsJson"></param>
		[JSInvokable(DevToolsCallbackId)]
		public async Task DevToolsCallback(string messageAsJson)
		{
			if (string.IsNullOrWhiteSpace(messageAsJson))
				return;

			var message = JsonSerialization.Deserialize<BaseCallbackObject>(messageAsJson);
			switch (message?.payload?.type)
			{
				case FromJsDevToolsDetectedActionTypeName:
					DevToolsBrowserPluginDetected = true;
					break;

				case "COMMIT":
					Func<Task> commit = OnCommit;
					if (commit is not null)
					{
						Task task = commit();
						if (task is not null)
							await task;
					}
					break;

				case "JUMP_TO_STATE":
				case "JUMP_TO_ACTION":
					Func<JumpToStateCallback, Task> jumpToState = OnJumpToState;
					if (jumpToState is not null)
					{
						var callbackInfo = JsonSerialization.Deserialize<JumpToStateCallback>(messageAsJson);
						Task task = jumpToState(callbackInfo);
						if (task is not null)
							await task;
					}
					break;
			}
		}

		void IDisposable.Dispose()
		{
			if (!Disposed)
			{
				DotNetRef.Dispose();
				Disposed = true;
			}
		}

		private static bool IsDotNetReferenceObject(object x) =>
			(x is not null)
			&& (x.GetType().IsGenericType)
			&& (x.GetType().GetGenericTypeDefinition() == typeof(DotNetObjectReference<>));

		private ValueTask<TResult> InvokeFluxorDevToolsMethodAsync<TResult>(string identifier, params object[] args)
		{
			if (!DevToolsBrowserPluginDetected && !IsInitializing)
				return new ValueTask<TResult>(default(TResult));


			if (args is not null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (!IsDotNetReferenceObject(args[i]))
						args[i] = JsonSerialization.Serialize(
							source: args[i],
							type: args[i]?.GetType() ?? typeof(object));
				}
			}

			string fullIdentifier = $"{FluxorDevToolsId}.{identifier}";
			return JSRuntime.InvokeAsync<TResult>(fullIdentifier, args);
		}

		internal static string GetClientScripts(ReduxDevToolsMiddlewareOptions options)
		{
			string optionsJson = BuildOptionsJson(options);

			return $@"
window.{FluxorDevToolsId} = new (function() {{
	const reduxDevTools = window.__REDUX_DEVTOOLS_EXTENSION__;
	this.{ToJsInitMethodName} = function() {{}};

	if (reduxDevTools !== undefined && reduxDevTools !== null) {{
		const fluxorDevTools = reduxDevTools.connect({{ {optionsJson} }});
		if (fluxorDevTools !== undefined && fluxorDevTools !== null) {{
			fluxorDevTools.subscribe((message) => {{ 
				if (window.fluxorDevToolsDotNetInterop) {{
					const messageAsJson = JSON.stringify(message);
					window.fluxorDevToolsDotNetInterop.invokeMethodAsync('{DevToolsCallbackId}', messageAsJson); 
				}}
			}});
		}}

		this.{ToJsInitMethodName} = function(dotNetCallbacks, state) {{
			window.fluxorDevToolsDotNetInterop = dotNetCallbacks;
			state = JSON.parse(state);
			fluxorDevTools.init(state);

			if (window.fluxorDevToolsDotNetInterop) {{
				// Notify Fluxor of the presence of the browser plugin
				const detectedMessage = {{
					payload: {{
						type: '{ReduxDevToolsInterop.FromJsDevToolsDetectedActionTypeName}'
					}}
				}};
				const detectedMessageAsJson = JSON.stringify(detectedMessage);
				window.fluxorDevToolsDotNetInterop.invokeMethodAsync('{DevToolsCallbackId}', detectedMessageAsJson);
			}}
		}};

		this.{ToJsDispatchMethodName} = function(action, state) {{
			action = JSON.parse(action);
			state = JSON.parse(state);
			fluxorDevTools.send(action, state);
		}};

	}}
}})();
";
		}

		private static string BuildOptionsJson(ReduxDevToolsMiddlewareOptions options)
		{
			var values = new List<string> {
				$"name:\"{options.Name}\"",
				$"maxAge:{options.MaximumHistoryLength}",
				$"latency:{options.Latency.TotalMilliseconds}"
			};
			return string.Join(",", values);
		}
	}
}
