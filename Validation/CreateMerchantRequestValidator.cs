using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class CreateMerchantRequestValidator : Validator<CreateMerchantRequest>
{
    public CreateMerchantRequestValidator()
    {
        RuleFor(r => r.Type)
            .NotNull().WithMessage("Type is required")
            .IsInEnum().WithMessage("Type is not a valid merchant type");
    }
}