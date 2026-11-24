namespace MiniLipila.Models.Requests;

public class DisbursementReq
{
    public Guid MerchantId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}
