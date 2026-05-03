using FluentValidation;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Presentation.Validators;

public class RecommendRecipesRequestValidator : AbstractValidator<RecommendRecipesRequestDto>
{
    private static readonly HashSet<string> AllowedDietary = new(StringComparer.OrdinalIgnoreCase)
    {
        "None", "Vegetarian", "Vegan"
    };

    public RecommendRecipesRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");

        When(x => x.IngredientHints is not null, () =>
        {
            RuleForEach(x => x.IngredientHints!)
                .NotEmpty()
                .WithMessage("Ingredient hint cannot be empty.")
                .MaximumLength(100)
                .WithMessage("Ingredient hint cannot exceed 100 characters.");
        });

        When(x => x.Preferences is not null, () =>
        {
            RuleFor(x => x.Preferences!.DietaryProfile)
                .Must(v => v == null || AllowedDietary.Contains(v))
                .WithMessage("DietaryProfile must be one of: None, Vegetarian, Vegan.");
        });

        When(x => x.Preferences?.ExcludedIngredients is not null, () =>
        {
            RuleForEach(x => x.Preferences!.ExcludedIngredients!)
                .NotEmpty()
                .WithMessage("Excluded ingredient cannot be empty.");
        });
    }
}