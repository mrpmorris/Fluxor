using Microsoft.AspNetCore.Components;
using Fluxor;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.EditUseCases;
using CustomerContracts = FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Pages.Customers
{
	public partial class Edit
	{
		[Inject] private IDispatcher Dispatcher { get; set; } = null!;
		[Inject] private IState<EditCustomerState> State { get; set; } = null!;

		[Parameter] public int CustomerId { get; set; }

		private CustomerContracts.EditCustomerDto? EditCustomerDto;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			SubscribeToAction<GetCustomerForEditActionResult>(x => EditCustomerDto = x.Dto);
			Dispatcher.Dispatch(new GetCustomerForEditAction(Id: CustomerId));
		}

		private void Save()
		{
			Dispatcher.Dispatch(new UpdateCustomerAction(EditCustomerDto!));
		}
	}
}