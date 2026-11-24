namespace MiniLipila.Models.Responses;

public class MerchantBalanceResponse
{
    public Guid MerchantId { get; set; }
    public int WalletCount { get; set; }
    public decimal TotalBalance { get; set; }
}
