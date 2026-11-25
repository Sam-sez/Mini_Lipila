namespace MiniLipila.Models.Responses;

public class LoginResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
}