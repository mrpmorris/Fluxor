using Fluxor.Blazor.Web.ReduxDevTools.CallbackObjects;
using Microsoft.JSInterop;
using Newtonsoft.Json;
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
		private readonly IJSRuntime JSRuntime;
		private bool IsInitializing;
		private DotNetObjectReference<ReduxDevToolsInterop> DotNetRef;

		/// <summary>
		/// Creates an instance of the dev tools interop
		/// </summary>
		/// <param name="jsRuntime"></param>
		public ReduxDevToolsInterop(IJSRuntime jsRuntime)
		{
			JSRuntime = jsRuntime;
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

		internal void Dispatch(object action, IDictionary<string, object> state)
		{
			InvokeFluxorDevToolsMethodAsync<object>(ToJsDispatchMethodName, new ActionInfo(action), state);
		}

		/// <summary>
		/// Called back from ReduxDevTools
		/// </summary>
		/// <param name="messageAsJson"></param>
		[JSInvokable(DevToolsCallbackId)]
		//TODO: Make private https://github.com/aspnet/Blazor/issues/1218
		public async Task DevToolsCallback(string messageAsJson)
		{
			if (string.IsNullOrWhiteSpace(messageAsJson))
				return;

			var message = JsonConvert.DeserializeObject<BaseCallbackObject>(messageAsJson);
			switch (message?.payload?.type)
			{
				case FromJsDevToolsDetectedActionTypeName:
					DevToolsBrowserPluginDetected = true;
					break;

				case "COMMIT":
					Func<Task> commit = OnCommit;
					if (commit != null)
					{
						Task task = commit();
						if (task != null)
							await task;
					}
					break;

				case "JUMP_TO_STATE":
				case "JUMP_TO_ACTION":
					Func<JumpToStateCallback, Task> jumpToState = OnJumpToState;
					if (jumpToState != null)
					{
						var callbackInfo = JsonConvert.DeserializeObject<JumpToStateCallback>(messageAsJson);
						Task task = jumpToState(callbackInfo);
						if (task != null)
							await task;
					}
					break;
			}
		}

#pragma warning disable CA1063 // Implement IDisposable Correctly
		void IDisposable.Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			DotNetRef.Dispose();
		}

		private ValueTask<TResult> InvokeFluxorDevToolsMethodAsync<TResult>(string identifier, params object[] args)
		{
			if (!DevToolsBrowserPluginDetected && !IsInitializing)
				return new ValueTask<TResult>(default(TResult));

			string fullIdentifier = $"{FluxorDevToolsId}.{identifier}";
			return JSRuntime.InvokeAsync<TResult>(fullIdentifier, args);
		}

		internal static string GetClientScripts()
		{
			string assemblyName = typeof(ReduxDevToolsInterop).Assembly.GetName().Name;

			return $@"
window.{FluxorDevToolsId} = new (function() {{
	const reduxDevTools = window.__REDUX_DEVTOOLS_EXTENSION__;
	this.{ToJsInitMethodName} = function() {{}};

	if (reduxDevTools !== undefined && reduxDevTools !== null) {{
		const fluxorDevTools = reduxDevTools.connect({{ name: 'Blazor-Fluxor' }});
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
			fluxorDevTools.send(action, state);
		}};

	}}
}})();
";
		}
	}
}
