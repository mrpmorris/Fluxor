using CustomerContracts = FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store;

public record UpdateCustomerAction(CustomerContracts.EditCustomerDto Dto);