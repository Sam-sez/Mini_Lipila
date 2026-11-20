namespace MiniLipila.Models;

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<Wallet> Wallets { get; set; } = new();
}