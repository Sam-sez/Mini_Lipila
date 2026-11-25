using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class CollectionReqValidator : Validator<CollectionReq>
{
    public CollectionReqValidator()
    {
        RuleFor(r => r.Amount).ValidAmount();
        RuleFor(r => r.Narration).ValidNarration();

        // A phone number is only needed when no saved customer was chosen
        RuleFor(r => r.AccountNumber).ZambianPhone().When(r => r.CustomerId is null);
    }
}