using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store;

public record SearchCustomersActionResult(IEnumerable<Customer> Customers);