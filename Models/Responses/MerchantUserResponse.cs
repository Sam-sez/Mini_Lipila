namespace MiniLipila.Models.Responses;

public class MerchantUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid MerchantId { get; set; }
    public string Message { get; set; } = string.Empty;
}