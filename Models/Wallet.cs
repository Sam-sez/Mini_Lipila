namespace MiniLipila.Models;

public class Wallet
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }

    public Guid MerchantId { get; set; }
    public Merchant? Merchant { get; set; }
}