namespace MiniLipila.Models;

// The role names, written once. Use these everywhere instead of typing the text.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Merchant = "Merchant";
    public const string Customer = "Customer";

    public static readonly string[] All = [Admin, Merchant, Customer];
}