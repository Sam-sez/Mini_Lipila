namespace MiniLipila.Models.Requests;

public class CreateMerchantUserRequest
{
    public Guid MerchantId { get; set; }             // from the route
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}