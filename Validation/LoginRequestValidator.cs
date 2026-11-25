using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(r => r.Password).NotEmpty().WithMessage("Password is required");
    }
}