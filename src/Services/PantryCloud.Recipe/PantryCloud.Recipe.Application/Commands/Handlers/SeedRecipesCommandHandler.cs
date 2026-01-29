using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Commands;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Core.Entities;
using PantryCloud.Recipe.Core.Enums;
using Unit = PantryCloud.SharedKernel.Enums.Unit;

namespace PantryCloud.Recipe.Application.Commands.Handlers;

public class SeedRecipesCommandHandler(IRecipeRepository repository)
    : IRequestHandler<SeedRecipesCommand, ErrorOr<SeedRecipesResponseDto>>
{
    public async Task<ErrorOr<SeedRecipesResponseDto>> Handle(
        SeedRecipesCommand request,
        CancellationToken cancellationToken)
    {
        var recipes = GenerateSampleRecipes(request.Request.Count);
        
        return await repository.SeedRecipesAsync(recipes, cancellationToken);
    }

    private static List<Core.Entities.Recipe> GenerateSampleRecipes(int count)
    {
        var recipes = new List<Core.Entities.Recipe>
        {
            new Core.Entities.Recipe()
            {
                Title = "Pasta Carbonara",
                Description = "Classic Italian pasta dish with eggs, cheese, pancetta, and black pepper.",
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Spaghetti", Quantity = 400, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Eggs", Quantity = 4, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Pancetta", Quantity = 200, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Parmesan Cheese", Quantity = 100, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Black Pepper", Quantity = 1, Unit = Unit.Teaspoon, IsOptional = false }
                },
                Steps = new List<RecipeStep>
                {
                    new() { StepNumber = 1, Instruction = "Cook spaghetti according to package directions.", DurationMinutes = 10 },
                    new() { StepNumber = 2, Instruction = "Fry pancetta until crispy.", DurationMinutes = 5 },
                    new() { StepNumber = 3, Instruction = "Mix eggs and parmesan in a bowl.", DurationMinutes = 2 },
                    new() { StepNumber = 4, Instruction = "Combine hot pasta with pancetta, then mix in egg mixture off heat.", DurationMinutes = 2 }
                },
                Tags = new List<string> { "Italian", "Quick", "Comfort Food" },
                DietaryLabels = new List<string>(),
                Source = RecipeSource.Internal,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 15,
                Servings = 4
            },
            new()
            {
                Title = "Caesar Salad",
                Description = "Fresh romaine lettuce with Caesar dressing, croutons, and parmesan.",
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Romaine Lettuce", Quantity = 1, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Caesar Dressing", Quantity = 100, Unit = Unit.Milliliter, IsOptional = false },
                    new() { Name = "Croutons", Quantity = 50, Unit = Unit.Gram, IsOptional = true },
                    new() { Name = "Parmesan Cheese", Quantity = 30, Unit = Unit.Gram, IsOptional = false }
                },
                Steps = new List<RecipeStep>
                {
                    new() { StepNumber = 1, Instruction = "Wash and chop romaine lettuce.", DurationMinutes = 5 },
                    new() { StepNumber = 2, Instruction = "Toss lettuce with Caesar dressing.", DurationMinutes = 2 },
                    new() { StepNumber = 3, Instruction = "Top with croutons and parmesan cheese.", DurationMinutes = 1 }
                },
                Tags = new List<string> { "Salad", "Vegetarian", "Quick" },
                DietaryLabels = new List<string> { "Vegetarian" },
                Source = RecipeSource.Internal,
                PrepTimeMinutes = 5,
                CookTimeMinutes = 0,
                Servings = 2
            },
            new()
            {
                Title = "Chicken Stir Fry",
                Description = "Quick and healthy stir-fried chicken with vegetables.",
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Chicken Breast", Quantity = 500, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Bell Peppers", Quantity = 2, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Broccoli", Quantity = 200, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Soy Sauce", Quantity = 30, Unit = Unit.Milliliter, IsOptional = false },
                    new() { Name = "Garlic", Quantity = 3, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Ginger", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true }
                },
                Steps = new List<RecipeStep>
                {
                    new() { StepNumber = 1, Instruction = "Cut chicken into strips and marinate with soy sauce.", DurationMinutes = 10 },
                    new() { StepNumber = 2, Instruction = "Heat oil in a wok or large pan.", DurationMinutes = 1 },
                    new() { StepNumber = 3, Instruction = "Stir-fry chicken until cooked through.", DurationMinutes = 5 },
                    new() { StepNumber = 4, Instruction = "Add vegetables and stir-fry until tender.", DurationMinutes = 5 },
                    new() { StepNumber = 5, Instruction = "Season with garlic and ginger, serve hot.", DurationMinutes = 1 }
                },
                Tags = new List<string> { "Asian", "Healthy", "Quick" },
                DietaryLabels = new List<string>(),
                Source = RecipeSource.Internal,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 4
            },
            new()
            {
                Title = "Chocolate Chip Cookies",
                Description = "Classic homemade chocolate chip cookies.",
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Flour", Quantity = 250, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Butter", Quantity = 150, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Sugar", Quantity = 100, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Brown Sugar", Quantity = 100, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Eggs", Quantity = 1, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Chocolate Chips", Quantity = 200, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Vanilla Extract", Quantity = 1, Unit = Unit.Teaspoon, IsOptional = true }
                },
                Steps = new List<RecipeStep>
                {
                    new() { StepNumber = 1, Instruction = "Preheat oven to 180°C (350°F).", DurationMinutes = 5 },
                    new() { StepNumber = 2, Instruction = "Cream butter and sugars together.", DurationMinutes = 3 },
                    new() { StepNumber = 3, Instruction = "Beat in egg and vanilla extract.", DurationMinutes = 2 },
                    new() { StepNumber = 4, Instruction = "Mix in flour and chocolate chips.", DurationMinutes = 3 },
                    new() { StepNumber = 5, Instruction = "Drop spoonfuls onto baking sheet and bake for 10-12 minutes.", DurationMinutes = 12 }
                },
                Tags = new List<string> { "Dessert", "Baking", "Sweet" },
                DietaryLabels = new List<string>(),
                Source = RecipeSource.Internal,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 24
            },
            new()
            {
                Title = "Beef Tacos",
                Description = "Mexican-style beef tacos with fresh toppings.",
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Ground Beef", Quantity = 500, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Taco Shells", Quantity = 8, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Lettuce", Quantity = 100, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Tomatoes", Quantity = 2, Unit = Unit.Piece, IsOptional = false },
                    new() { Name = "Cheese", Quantity = 100, Unit = Unit.Gram, IsOptional = false },
                    new() { Name = "Sour Cream", Quantity = 100, Unit = Unit.Milliliter, IsOptional = true },
                    new() { Name = "Taco Seasoning", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = false }
                },
                Steps = new List<RecipeStep>
                {
                    new() { StepNumber = 1, Instruction = "Brown ground beef in a pan.", DurationMinutes = 8 },
                    new() { StepNumber = 2, Instruction = "Add taco seasoning and water, simmer.", DurationMinutes = 5 },
                    new() { StepNumber = 3, Instruction = "Chop lettuce and tomatoes.", DurationMinutes = 5 },
                    new() { StepNumber = 4, Instruction = "Warm taco shells in oven.", DurationMinutes = 3 },
                    new() { StepNumber = 5, Instruction = "Fill shells with beef and toppings.", DurationMinutes = 2 }
                },
                Tags = new List<string> { "Mexican", "Comfort Food", "Quick" },
                DietaryLabels = new List<string>(),
                Source = RecipeSource.Internal,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 16,
                Servings = 4
            }
        };

        // Return requested count (or all if count is larger)
        return recipes.Take(count).ToList();
    }
}

