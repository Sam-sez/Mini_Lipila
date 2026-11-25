namespace MiniLipila.Models.Requests;

// What a person sends to log in.
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}