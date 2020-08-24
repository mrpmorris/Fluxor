using Fluxor.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web
{
	/// <summary>
	/// Initializes the store for the current user. This should be placed in the App.razor component.
	/// </summary>
	public class StoreInitializer : ComponentBase, IDisposable
	{
		[Parameter]
		public EventCallback<Exceptions.UnhandledExceptionEventArgs> UnhandledException { get; set; }

		[Inject]
		private IStore Store { get; set; }

		[Inject]
		private IJSRuntime JSRuntime { get; set; }

		private string Scripts;
		private bool IsDisposed;
		private Exception ExceptionToThrow;

		/// <summary>
		/// Retrieves supporting JavaScript for any Middleware
		/// </summary>
		protected override void OnInitialized()
		{
			Store.UnhandledException += OnUnhandledException;

			var webMiddlewares = Store.GetMiddlewares().OfType<IWebMiddleware>();

			var scriptBuilder = new StringBuilder();
			scriptBuilder.AppendLine("<script id='initializeFluxor'>");
			{
				foreach (IWebMiddleware middleware in webMiddlewares)
				{
					string script = middleware.GetClientScripts();
					if (script != null)
					{
						scriptBuilder.AppendLine($"// Middleware scripts: {middleware.GetType().FullName}");
						scriptBuilder.AppendLine(script);
					}
				}
			}
			scriptBuilder.AppendLine("</script>");
			Scripts = scriptBuilder.ToString();
			base.OnInitialized();
		}

		/// <summary>
		/// Renders the supporting JavaScript for any Middleware
		/// </summary>
		/// <param name="builder">The builder</param>
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);
			builder.AddMarkupContent(0, Scripts);
		}

		protected override void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
			if (ExceptionToThrow != null)
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
					bool success = await JSRuntime.InvokeAsync<bool>("tryInitializeFluxor");
					if (!success)
						throw new StoreInitializationException("Failed to initialize store");

					await Store.InitializeAsync();
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

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Store.UnhandledException -= OnUnhandledException;
				}
				IsDisposed = true;
			}
		}

		private void OnUnhandledException(object sender, Exceptions.UnhandledExceptionEventArgs args)
		{
			InvokeAsync(async () =>
			{
				Exception exceptionThrownInHandler = null;
				try
				{
					await UnhandledException.InvokeAsync(args).ConfigureAwait(false);
				}
				catch(Exception e)
				{
					exceptionThrownInHandler = e;
				}

				if (exceptionThrownInHandler != null || !args.WasHandled)
				{
					ExceptionToThrow = exceptionThrownInHandler ?? args.Exception;
					StateHasChanged();
				}
			});
		}

		void IDisposable.Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
