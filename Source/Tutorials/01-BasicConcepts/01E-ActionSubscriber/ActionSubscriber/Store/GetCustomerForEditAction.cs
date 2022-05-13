using System;

namespace BasicConcepts.ActionSubscriber.Store
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
