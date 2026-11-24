namespace MiniLipila.Models.Responses;

public class DisbursementResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
}
