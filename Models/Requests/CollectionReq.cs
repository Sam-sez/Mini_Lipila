namespace MiniLipila.Models.Requests;

public class CollectionReq
{
    public Guid MerchantId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public Guid? CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}
