using BasicConcepts.ActionSubscriberTutorial.ApiObjects;

namespace BasicConcepts.ActionSubscriberTutorial.Store
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
