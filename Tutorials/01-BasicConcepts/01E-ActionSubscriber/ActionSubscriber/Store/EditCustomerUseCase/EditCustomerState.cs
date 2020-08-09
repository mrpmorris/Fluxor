namespace BasicConcepts.ActionSubscriber.Store.EditCustomerUseCase
{
	public class EditCustomerState
	{
		public bool IsLoading { get; private set; }

		public EditCustomerState(bool isLoading)
		{
			IsLoading = isLoading;
		}
	}
}
