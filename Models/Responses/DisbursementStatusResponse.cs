namespace MiniLipila.Models.Responses;

public class DisbursementStatusResponse
{
    public Guid PaymentId { get; set; }
    public string OurStatus { get; set; } = string.Empty;
    public string LipilaStatus { get; set; } = string.Empty;
    public bool WalletRefunded { get; set; }
    public string Message { get; set; } = string.Empty;
}
