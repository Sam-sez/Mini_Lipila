using System.Text.Json.Serialization;

namespace MiniLipila.Models.Lipila;

public class LipilaKycResponse
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
}