using PostgresEnumExtension;

namespace MiniLipila.Models;

public class Payment : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public Guid WalletId { get; set; }
    public Wallet? Wallet { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }
    public required string AccountNumber { get; set; } // the customer's phone number
    public string Narration { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public PaymentType Type { get; set; }
}

[PostgresEnum]
public enum PaymentStatus
{
    Pending,
    Successful,
    Failed
}

[PostgresEnum]
public enum PaymentType
{
    Collection,
    Disbursement
}