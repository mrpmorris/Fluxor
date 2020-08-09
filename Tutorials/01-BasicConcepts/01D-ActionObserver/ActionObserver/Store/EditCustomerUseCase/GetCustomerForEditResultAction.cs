using BasicConcepts.ActionObserver.ApiObjects;

namespace BasicConcepts.ActionObserver.Store.EditCustomerUseCase
{
	public class GetCustomerForEditResultAction
	{
		public CustomerEdit Customer { get; }

		public GetCustomerForEditResultAction(CustomerEdit customer)
		{
			Customer = customer;
		}
	}
}
