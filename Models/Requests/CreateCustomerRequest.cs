namespace MiniLipila.Models.Requests;

public class CreateCustomerRequest
{
    public Guid MerchantId { get; set; }   // comes from the address
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
