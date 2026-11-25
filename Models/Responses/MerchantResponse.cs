namespace MiniLipila.Models.Responses;

public class MerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MerchantType Type { get; set; }

    public List<WalletResponse> Wallets { get; set; } = new();
}