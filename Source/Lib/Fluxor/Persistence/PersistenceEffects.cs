using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxor.Persistence
{
	public sealed class PersistenceEffects
	{
		private readonly IPersistenceManager _persistenceManager;
		private readonly IServiceProvider _serviceProvider;

		public PersistenceEffects(IPersistenceManager persistenceManager, IServiceProvider serviceProvider)
		{
			_persistenceManager = persistenceManager;
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Maintains a reference to IStore - injected this way to avoid a circular dependency during the effect method registration
		/// </summary>
		private readonly Lazy<IStore> _store = new(_serviceProvider.GetRequiredService<IStore>);

		[EffectMethod(typeof(StorePersistingAction))]
		public async Task PersistStoreData(IDispatcher dispatcher)
		{
			//Serialize the store
			var json = _store.Value.SerializeToJson();

			//Save to the persistence manager
			await _persistenceManager.PersistStoreToStateAsync(json);

			//Completed
			dispatcher.Dispatch(new StorePersistedAction());
		}

		[EffectMethod(typeof(StoreRehydratingAction))]
		public async Task RehydrateStoreData(IDispatcher dispatcher)
		{
			//Read from the persistence manager
			var serializedStore = await _persistenceManager.RehydrateStoreFromStateAsync();
			if (serializedStore is null)
			{
				//Nothing to rehydrate - leave as-is
				dispatcher.Dispatch(new StoreRehydratedAction());
				return;
			}

			_store.Value.RehydrateFromJson(serializedStore);

			//Completed
			dispatcher.Dispatch(new StoreRehydratedAction());
		}
	}
}
