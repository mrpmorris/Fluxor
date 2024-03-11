#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxor.Persistence
{
	public interface IPersistenceManager
	{
		/// <summary>
		/// Persists the store to a persisted state.
		/// </summary>
		/// <param name="serializedStore">The serialized store data being persisted.</param>
		public Task PersistStoreToStateAsync(string serializedStore);

		/// <summary>
		/// Rehydrates the store from a persisted state.
		/// </summary>
		public Task<string?> RehydrateStoreFromStateAsync();

		/// <summary>
		/// Clears the store from the persisted state.
		/// </summary>
		public Task ClearStoreFromStateAsync();
	}
}
