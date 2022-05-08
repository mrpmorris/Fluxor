namespace FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;

public class EditCustomerDto
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? EmailAddress { get; set; }
	public string? Notes { get; set; }

	public EditCustomerDto() { }

	public EditCustomerDto(int id, string? name, string? emailAddress, string? notes = null)
	{
		Id = id;
		Name = name;
		EmailAddress = emailAddress;
		Notes = notes;
	}
}
