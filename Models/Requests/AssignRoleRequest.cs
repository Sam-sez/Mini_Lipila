namespace MiniLipila.Models.Requests;

public class AssignRoleRequest
{
    public string Id { get; set; } = string.Empty;   // from the route
    public string Role { get; set; } = string.Empty; // from the body
}