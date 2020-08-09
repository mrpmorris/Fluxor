using BasicConcepts.ActionSubscriber.ApiObjects;

namespace BasicConcepts.ActionSubscriber.Store.EditCustomerUseCase
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
