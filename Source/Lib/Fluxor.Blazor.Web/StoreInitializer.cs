using Fluxor.Blazor.Web.Components;
using Fluxor.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web;

/// <summary>
/// Initializes the store for the current user. This should be placed in the App.razor component.
/// </summary>
public class StoreInitializer : FluxorComponent
{
#if NET9_0_OR_GREATER
	private const string PersistenceStateKey = "Fluxor.StoreState";

	[Parameter]
	public bool DisableServerSideRendering { get; set; }
#endif

	[Parameter]
	public EventCallback<Exceptions.UnhandledExceptionEventArgs> UnhandledException { get; set; }

	[Inject]
	private IJSRuntime JSRuntime { get; set; }

	[Inject]
	private PersistentComponentState PersistentComponentState { get; set; }

	[Inject]
	private IStore Store { get; set; }

	private string MiddlewareInitializationScripts;
	private Exception ExceptionToThrow;

	/// <summary>
	/// Disposes via IAsyncDisposable
	/// </summary>
	/// <param name="disposing">true if called manually, otherwise false</param>
	protected override ValueTask DisposeAsyncCore(bool disposing)
	{
		if (disposing)
			Store.UnhandledException -= OnUnhandledException;
		return base.DisposeAsyncCore(disposing);
	}

	/// <summary>
	/// Retrieves supporting JavaScript for any Middleware
	/// </summary>
	protected override Task OnInitializedAsync()
	{
		Store.UnhandledException += OnUnhandledException;

		var webMiddlewares = Store.GetMiddlewares().OfType<IWebMiddleware>();

		var scriptBuilder = new StringBuilder();
		foreach (IWebMiddleware middleware in webMiddlewares)
		{
			string script = middleware.GetClientScripts();
			if (script is not null)
			{
				scriptBuilder.AppendLine($"// Middleware scripts: {middleware.GetType().FullName}");
				scriptBuilder.AppendLine(script);
			}
		}
		MiddlewareInitializationScripts = scriptBuilder.ToString();
		base.OnInitialized();

#if NET9_0_OR_GREATER
		return HandleServerSideRenderingAsync();
#else
		return Task.CompletedTask;
#endif
	}

	protected override void OnAfterRender(bool firstRender)
	{
		base.OnAfterRender(firstRender);
		if (ExceptionToThrow is not null)
		{
			Exception exception = ExceptionToThrow;
			ExceptionToThrow = null;
			throw exception;
		}
	}

	/// <summary>
	/// Executes any supporting JavaScript required for Middleware
	/// </summary>
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(MiddlewareInitializationScripts))
					await JSRuntime.InvokeVoidAsync("eval", MiddlewareInitializationScripts);

				await DeserializeStoreStateAndInitializeAsync();
			}
			catch (JSException err)
			{
				// An error in some JavaScript, cannot recover from this
				throw new StoreInitializationException("JavaScript error", err);
			}
			catch (TaskCanceledException)
			{
				// The browser has disconnected from a server-side-blazor app and can no longer be reached.
				// Swallow this exception as the store will be abandoned and garbage collected.
				return;
			}
			catch (Exception err)
			{
				throw new StoreInitializationException("Store initialization error", err);
			}
		}
	}

	private async Task DeserializeStoreStateAndInitializeAsync()
	{
#if NET9_0_OR_GREATER
		Console.WriteLine("Deserializing");
		if (PersistentComponentState.TryTakeFromJson(
			key: PersistenceStateKey,
			instance: out IDictionary<string, JsonElement> persistedState))
		{
			var stateDict = new Dictionary<string, object>();
			foreach(var kvp in Store.Features)
			{
				if (persistedState.TryGetValue(kvp.Key, out JsonElement persistedFeatureStateJsonElement))
				{
					object featureState = persistedFeatureStateJsonElement.Deserialize(kvp.Value.GetStateType());
					stateDict[kvp.Key] = featureState;
				}
			}
			foreach (var kvp in persistedState)
				Console.WriteLine($"{kvp.Value.GetType().Name} {kvp.Key} = {kvp.Value}");
			await Store.InitializeAsync(stateDict);
		}
#else
		await Store.InitializeAsync();
#endif
	}

#if NET9_0_OR_GREATER
	private async Task HandleServerSideRenderingAsync()
	{
		Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(RendererInfo));
		await InitializeAndSerializeStoreStateAsync();
	}

	private async Task InitializeAndSerializeStoreStateAsync()
	{
		if (DisableServerSideRendering || RendererInfo.IsInteractive || RendererInfo.Name != "Static")
			return;


		PersistentComponentState.RegisterOnPersisting(
			() =>
			{
				Console.WriteLine("Serializing");
				FrozenDictionary<string, object> storeState =
					Store.GetState(onlyDebuggerBrowsable: false);

				PersistentComponentState.PersistAsJson(
					key: PersistenceStateKey,
					instance: storeState
				);
				return Task.CompletedTask;
			}
		);

		Console.WriteLine("Initializing");
		await Store.InitializeAsync();
		await Store.Initialized;
	}
#endif

	private void OnUnhandledException(object sender, Exceptions.UnhandledExceptionEventArgs e)
	{
		InvokeAsync(async () =>
		{
			Exception exceptionThrownInHandler = null;
			try
			{
				await UnhandledException.InvokeAsync(e).ConfigureAwait(false);
			}
			catch (Exception exception)
			{
				exceptionThrownInHandler = exception;
			}

			if (exceptionThrownInHandler is not null || !e.WasHandled)
			{
				ExceptionToThrow = exceptionThrownInHandler ?? e.Exception;
				StateHasChanged();
			}
		});
	}
}
