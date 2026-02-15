using FluentValidation;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Presentation.Validators;

public class SearchRecipesRequestValidator : AbstractValidator<SearchRecipesRequestDto>
{
    private static readonly HashSet<string> AllowedDietary = new(StringComparer.OrdinalIgnoreCase)
    {
        "None", "Vegetarian", "Vegan"
    };

    public SearchRecipesRequestValidator()
    {
        RuleFor(x => x.Ingredients)
            .NotNull()
            .WithMessage("Ingredients list is required.")
            .NotEmpty()
            .WithMessage("At least one ingredient is required.");

        RuleForEach(x => x.Ingredients)
            .NotEmpty()
            .WithMessage("Ingredient name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Ingredient name cannot exceed 100 characters.");

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