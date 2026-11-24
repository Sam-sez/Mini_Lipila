namespace MiniLipila.Models.Lipila;

public class PaymentRequest
{
    public string ReferenceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ReferenceData { get; set; } = string.Empty;
}
