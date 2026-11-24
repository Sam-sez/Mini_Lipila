using System.Text.Json.Serialization;

namespace MiniLipila.Models.Lipila;

public class LipilaPaymentResponse
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "ZMW";

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("paymentType")]
    public string PaymentType { get; set; } = string.Empty;

    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("cardRedirectionUrl")]
    public string? CardRedirectionUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; set; } = string.Empty;

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
