﻿using BasicConcepts.ActionObserver.ApiObjects;
using Fluxor;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.ActionObserver.Store.EditCustomerUseCase
{
	public class Effects
	{
		[EffectMethod]
		public async Task HandleGetCustomerForEditAction(GetCustomerForEditAction action, IDispatcher dispatcher)
		{
			string id = action.Id.ToString();
			Console.WriteLine("Getting customer with Id: " + action.Id.ToString());

			await Task.Delay(1000);

			string jsonFromServer = $"{{\"Id\":\"{id}\",\"RowVersion\":\"AQIDBAUGBwgJCgsMDQ4PEA==\",\"Name\":\"Our first customer\"}}";
			var objectFromServer = JsonConvert.DeserializeObject<CustomerEdit>(jsonFromServer);
			dispatcher.Dispatch(new GetCustomerForEditResultAction(objectFromServer));
		}
	}
}