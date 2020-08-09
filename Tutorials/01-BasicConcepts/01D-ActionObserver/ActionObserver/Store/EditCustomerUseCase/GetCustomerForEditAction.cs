using System;

namespace BasicConcepts.ActionObserver.Store.EditCustomerUseCase
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
