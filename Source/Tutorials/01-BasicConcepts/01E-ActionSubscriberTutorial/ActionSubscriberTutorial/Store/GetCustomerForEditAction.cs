using System;

namespace BasicConcepts.ActionSubscriberTutorial.Store
{
	public class GetCustomerForEditAction
	{
		public Guid Id { get; }

		public GetCustomerForEditAction(Guid id)
		{
			Id = id;
		}
	}
}
