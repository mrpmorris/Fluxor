using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.Blazor.Web.Components
{
	/// <summary>
	/// A component that auto-subscribes to state changes on all <see cref="IState"/> properties
	/// and ensures <see cref="ComponentBase.StateHasChanged"/> is called
	/// </summary>
	public class FluxorComponent : ComponentBase, IDisposable
	{
		private bool Disposed;
		private List<IState> SubscribedStates = new List<IState>();

		/// <summary>
		/// Disposes of the component and unsubscribes from any state
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Subscribes to state properties
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			// Find all state properties
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			IEnumerable<PropertyInfo> stateProperties = GetType().GetProperties(bindingFlags)
				.Where(t => typeof(IState).IsAssignableFrom(t.PropertyType));
			// Subscribe to each state so that StateHasChanged is executed when the state changes
			foreach (PropertyInfo propertyInfo in stateProperties)
			{
				IState state = (IState)propertyInfo.GetValue(this);
				SubscribedStates.Add(state);
				state.StateChanged += InvokeStateHasChanged;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					UnsubscribeFromState();
				}
				Disposed = true;
			}
		}

		private void InvokeStateHasChanged(object sender, EventArgs e)
		{
			InvokeAsync(StateHasChanged);
		}

		private void UnsubscribeFromState()
		{
			foreach (IState state in SubscribedStates)
				state.StateChanged -= InvokeStateHasChanged;
		}

	}
}
