namespace MiniLipila.Models;

public class Payment : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }
    public required string AccountNumber { get; set; } // the customer's phone number
    public string Narration { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    
    public PaymentType Type { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed
}

public enum PaymentType
{
    Collection,
    Disbursement
}