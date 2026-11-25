namespace MiniLipila.Models.Requests;

// What a person sends to create an account.
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}