namespace MiniLipila.Models.Lipila;

public class LipilaDisbursementBody
{
    public string ReferenceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}
