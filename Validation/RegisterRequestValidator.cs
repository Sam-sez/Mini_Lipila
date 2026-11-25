using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Enter a valid email address");

        // Strength rules are enforced by Identity (see Program.cs), so we only check it was sent
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}