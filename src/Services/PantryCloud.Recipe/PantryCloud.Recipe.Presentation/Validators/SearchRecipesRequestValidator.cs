using FluentValidation;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Presentation.Validators;

public abstract class SearchRecipesRequestValidator : AbstractValidator<SearchRecipesRequestDto>
{
    protected SearchRecipesRequestValidator()
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
    }
}


