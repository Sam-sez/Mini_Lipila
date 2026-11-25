using FluentValidation;
using MiniLipila.Modules;

namespace MiniLipila.Validation;

// Every shared rule lives here, written once.
// Validators below pick the rules they need.
public static class CommonRules
{
    public static IRuleBuilderOptions<T, decimal> ValidAmount<T>(this IRuleBuilder<T, decimal> rule)
        => rule.GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

    public static IRuleBuilderOptions<T, string> ZambianPhone<T>(this IRuleBuilder<T, string> rule)
        => rule.Must(phone => !string.IsNullOrWhiteSpace(phone) && Phone.ToLipilaFormat(phone) is not null)
            .WithMessage("Enter a Zambian number like 0979086825 or 260979086825");

    public static IRuleBuilderOptions<T, string> ValidNarration<T>(this IRuleBuilder<T, string> rule)
        => rule.Must(text => text is not null && text.Trim().Length is >= 3 and <= 200)
            .WithMessage("Narration must be 3 to 200 characters");

    public static IRuleBuilderOptions<T, string> RequiredName<T>(this IRuleBuilder<T, string> rule)
        => rule.Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required");
}