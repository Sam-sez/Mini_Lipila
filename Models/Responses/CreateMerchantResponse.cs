namespace MiniLipila.Models.Responses;

public class CreateMerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<WalletResponse> Wallets { get; set; } = new();
}
