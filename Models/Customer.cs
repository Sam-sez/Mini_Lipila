using PostgresEnumExtension;

namespace MiniLipila.Models;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public CustomerType CustomerType { get; set; }

    // a customer can be linked to many merchants
    public List<Merchant> Merchants { get; set; } = new();
}

[PostgresEnum]
public enum CustomerType
{
    Individual,
    Company
}