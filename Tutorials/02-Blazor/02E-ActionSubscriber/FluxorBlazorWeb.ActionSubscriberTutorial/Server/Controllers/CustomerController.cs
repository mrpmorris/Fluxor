using CustomerContracts = FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Server.Controllers
{
	[ApiController]
	public class CustomerController : ControllerBase
	{
		private static readonly ConcurrentDictionary<int, CustomerContracts.EditCustomerDto> CustomersById;

		[HttpGet, Route("/api/customers/search")]
		public IEnumerable<CustomerContracts.CustomerSummaryDto> Search()
		{
			return CustomersById.Values
				.OrderBy(x => x.Id)
				.Select(x => new CustomerContracts.CustomerSummaryDto(Id: x.Id, Name: x.Name))
				.ToArray();
		}

		[HttpGet, Route("/api/customer/{customerId:int}")]
		public CustomerContracts.EditCustomerDto Get(int customerId) =>
			CustomersById.TryGetValue(customerId, out var customer)
				? customer
				: throw new KeyNotFoundException();

		[HttpPost, Route("/api/customer/")]
		public IActionResult Update(CustomerContracts.EditCustomerDto customer)
		{
			CustomersById[customer.Id] = customer;
			return Ok();
		}

		static CustomerController()
		{
			CustomersById = new ConcurrentDictionary<int, CustomerContracts.EditCustomerDto>()
			{
				[0] = new(id: 0, name: "Peter Morris", emailAddress: "mrpmorris@gmail.com"),
				[1] = new(id: 1, name: "Steven Cramer", emailAddress: "Steve@Cramer.com", notes: "A bit smelly"),
				[2] = new(id: 2, name: "Bob Morris", emailAddress: "bob@morris.com"),
				[3] = new(id: 3, name: "Bob Smith", emailAddress: "bob@smith.com"),
				[4] = new(id: 4, name: "John Smith", emailAddress: "john@smith.com")
			};
		}
	}
}
