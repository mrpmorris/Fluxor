using BasicConcepts.ActionSubscriber.ApiObjects;
using Fluxor;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.ActionSubscriber.Store.EditCustomerUseCase
{
	public class Effects
	{
		[EffectMethod]
		public async Task HandleGetCustomerForEditAction(GetCustomerForEditAction action, IDispatcher dispatcher)
		{
			Console.WriteLine("Getting customer with Id: 42");

			await Task.Delay(1000);

			string jsonFromServer = $"{{\"Id\":42,\"RowVersion\":\"AQIDBAUGBwgJCgsMDQ4PEA==\",\"Name\":\"Our first customer\"}}";
			var objectFromServer = JsonConvert.DeserializeObject<CustomerEdit>(jsonFromServer);
			dispatcher.Dispatch(new GetCustomerForEditResultAction(objectFromServer));
		}
	}
}
