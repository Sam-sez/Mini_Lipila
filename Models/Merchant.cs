using MiniLipila.Modules.Endpoints;

namespace MiniLipila.Models;

public class Merchant: BaseEntity
{

    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<Wallet> Wallets { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
}


public class MerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<WalletResponse> Wallets { get; set; } = new();
}