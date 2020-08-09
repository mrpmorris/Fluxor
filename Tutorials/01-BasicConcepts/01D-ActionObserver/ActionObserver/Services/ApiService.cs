using BasicConcepts.ActionObserver.ApiObjects;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.ActionObserver.Services
{
	public interface IApiService
	{
		Task<CustomerEdit> GetCustomerToEditAsync(Guid id);
	}

	public class ApiService : IApiService
	{
		public Task<CustomerEdit> GetCustomerToEditAsync(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
