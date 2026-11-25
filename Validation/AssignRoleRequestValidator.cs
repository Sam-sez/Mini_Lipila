using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class AssignRoleRequestValidator : Validator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        // only the three real role names are allowed
        RuleFor(r => r.Role)
            .Must(role => MiniLipila.Models.Roles.All.Contains(role))
            .WithMessage("Role must be Admin, Merchant or Customer");
    }
}