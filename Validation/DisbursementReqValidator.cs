using FastEndpoints;
using FluentValidation;
using MiniLipila.Models.Requests;

namespace MiniLipila.Validation;

public class DisbursementReqValidator : Validator<DisbursementReq>
{
    public DisbursementReqValidator()
    {
        RuleFor(r => r.Amount).ValidAmount();
        RuleFor(r => r.AccountNumber).ZambianPhone();
        RuleFor(r => r.Narration).ValidNarration();
    }
}