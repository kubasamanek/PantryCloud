using PantryCloud.Recipe.Core.Entities;
using PantryCloud.Recipe.Core.Enums;
using Unit = PantryCloud.SharedKernel.Enums.Unit;

namespace PantryCloud.Recipe.Application.Data;

/// <summary>
/// Default recipes seeded into the database on first startup.
/// Used by both the startup seeder and the dev-only /seed endpoint.
/// </summary>
public static class DefaultRecipeData
{
    public static List<Core.Entities.Recipe> GetDefaultRecipes() =>
    [
        new()
        {
            Title = "Pasta Carbonara",
            Description = "Classic Italian pasta dish with eggs, cheese, pancetta, and black pepper.",
            Ingredients =
            [
                new() { Name = "Spaghetti", Quantity = 400, Unit = Unit.Gram },
                new() { Name = "Eggs", Quantity = 4, Unit = Unit.Piece },
                new() { Name = "Pancetta", Quantity = 200, Unit = Unit.Gram },
                new() { Name = "Parmesan Cheese", Quantity = 100, Unit = Unit.Gram },
                new() { Name = "Black Pepper", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Salt", Quantity = 1, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Bring a large pot of salted water to a boil and cook spaghetti until al dente.", DurationMinutes = 10 },
                new() { StepNumber = 2, Instruction = "Fry pancetta in a pan until crispy, then set aside.", DurationMinutes = 5 },
                new() { StepNumber = 3, Instruction = "Whisk eggs with grated parmesan and a generous amount of black pepper.", DurationMinutes = 2 },
                new() { StepNumber = 4, Instruction = "Drain pasta, reserving a cup of pasta water. Combine hot pasta with pancetta off the heat.", DurationMinutes = 1 },
                new() { StepNumber = 5, Instruction = "Pour the egg mixture over the pasta, tossing quickly. Add pasta water gradually to achieve a creamy sauce.", DurationMinutes = 2 }
            ],
            Tags = ["Italian", "Pasta", "Comfort Food"],
            DietaryLabels = [],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 15,
            Servings = 4
        },
        new()
        {
            Title = "Caesar Salad",
            Description = "Crisp romaine lettuce tossed with Caesar dressing, crunchy croutons, and shaved parmesan.",
            Ingredients =
            [
                new() { Name = "Romaine Lettuce", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Caesar Dressing", Quantity = 100, Unit = Unit.Milliliter },
                new() { Name = "Croutons", Quantity = 50, Unit = Unit.Gram, IsOptional = true },
                new() { Name = "Parmesan Cheese", Quantity = 30, Unit = Unit.Gram },
                new() { Name = "Lemon Juice", Quantity = 1, Unit = Unit.Tablespoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Wash, dry, and tear romaine lettuce into bite-sized pieces.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Toss lettuce with Caesar dressing and a squeeze of lemon juice.", DurationMinutes = 2 },
                new() { StepNumber = 3, Instruction = "Top with croutons and shaved parmesan. Serve immediately.", DurationMinutes = 1 }
            ],
            Tags = ["Salad", "Quick", "Lunch"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 5,
            CookTimeMinutes = 0,
            Servings = 2
        },
        new()
        {
            Title = "Chicken Stir Fry",
            Description = "Quick and healthy stir-fried chicken with colourful vegetables in a savoury soy-ginger sauce.",
            Ingredients =
            [
                new() { Name = "Chicken Breast", Quantity = 500, Unit = Unit.Gram },
                new() { Name = "Bell Peppers", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Broccoli", Quantity = 200, Unit = Unit.Gram },
                new() { Name = "Soy Sauce", Quantity = 3, Unit = Unit.Tablespoon },
                new() { Name = "Garlic", Quantity = 3, Unit = Unit.Piece },
                new() { Name = "Ginger", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true },
                new() { Name = "Sesame Oil", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Vegetable Oil", Quantity = 2, Unit = Unit.Tablespoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Slice chicken breast into thin strips and marinate with half the soy sauce for 10 minutes.", DurationMinutes = 10 },
                new() { StepNumber = 2, Instruction = "Heat vegetable oil in a wok over high heat.", DurationMinutes = 1 },
                new() { StepNumber = 3, Instruction = "Stir-fry chicken strips until golden and cooked through, then remove from wok.", DurationMinutes = 5 },
                new() { StepNumber = 4, Instruction = "Add garlic and ginger to the wok; stir 30 seconds. Add vegetables and stir-fry until tender-crisp.", DurationMinutes = 4 },
                new() { StepNumber = 5, Instruction = "Return chicken to wok, add remaining soy sauce and sesame oil. Toss and serve with rice.", DurationMinutes = 2 }
            ],
            Tags = ["Asian", "Healthy", "Quick", "Dinner"],
            DietaryLabels = [],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 15,
            CookTimeMinutes = 12,
            Servings = 4
        },
        new()
        {
            Title = "Chocolate Chip Cookies",
            Description = "Soft, golden-edged homemade cookies loaded with chocolate chips.",
            Ingredients =
            [
                new() { Name = "Flour", Quantity = 250, Unit = Unit.Gram },
                new() { Name = "Butter", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Sugar", Quantity = 100, Unit = Unit.Gram },
                new() { Name = "Brown Sugar", Quantity = 100, Unit = Unit.Gram },
                new() { Name = "Eggs", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Chocolate Chips", Quantity = 200, Unit = Unit.Gram },
                new() { Name = "Vanilla Extract", Quantity = 1, Unit = Unit.Teaspoon, IsOptional = true },
                new() { Name = "Baking Soda", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Salt", Quantity = 0.5m, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Preheat oven to 180 °C (350 °F) and line a baking sheet with parchment paper.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Beat softened butter with both sugars until light and fluffy.", DurationMinutes = 3 },
                new() { StepNumber = 3, Instruction = "Beat in the egg and vanilla extract.", DurationMinutes = 1 },
                new() { StepNumber = 4, Instruction = "Stir in flour, baking soda, and salt until just combined. Fold in chocolate chips.", DurationMinutes = 3 },
                new() { StepNumber = 5, Instruction = "Drop tablespoon-sized balls onto the baking sheet. Bake 10–12 minutes until edges are golden.", DurationMinutes = 12 },
                new() { StepNumber = 6, Instruction = "Cool on the sheet for 5 minutes before transferring to a rack.", DurationMinutes = 5 }
            ],
            Tags = ["Dessert", "Baking", "Sweet"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 15,
            CookTimeMinutes = 12,
            Servings = 24
        },
        new()
        {
            Title = "Beef Tacos",
            Description = "Juicy seasoned ground beef in crispy shells topped with fresh salsa, cheese, and sour cream.",
            Ingredients =
            [
                new() { Name = "Ground Beef", Quantity = 500, Unit = Unit.Gram },
                new() { Name = "Taco Shells", Quantity = 8, Unit = Unit.Piece },
                new() { Name = "Lettuce", Quantity = 100, Unit = Unit.Gram },
                new() { Name = "Tomatoes", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Cheddar Cheese", Quantity = 100, Unit = Unit.Gram },
                new() { Name = "Sour Cream", Quantity = 100, Unit = Unit.Milliliter, IsOptional = true },
                new() { Name = "Taco Seasoning", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Onion", Quantity = 1, Unit = Unit.Piece }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Dice onion and tomatoes; shred lettuce. Set aside.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Brown ground beef with diced onion in a pan over medium-high heat, breaking it up.", DurationMinutes = 8 },
                new() { StepNumber = 3, Instruction = "Drain excess fat. Add taco seasoning and 60 ml water; simmer 3 minutes.", DurationMinutes = 4 },
                new() { StepNumber = 4, Instruction = "Warm taco shells in the oven at 180 °C for 3 minutes.", DurationMinutes = 3 },
                new() { StepNumber = 5, Instruction = "Fill each shell with beef mixture. Top with lettuce, tomatoes, cheese, and sour cream.", DurationMinutes = 3 }
            ],
            Tags = ["Mexican", "Comfort Food", "Quick", "Dinner"],
            DietaryLabels = [],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 15,
            Servings = 4
        },
        new()
        {
            Title = "Vegetable Curry",
            Description = "Hearty Indian-inspired chickpea and vegetable curry simmered in a fragrant tomato-coconut sauce.",
            Ingredients =
            [
                new() { Name = "Chickpeas", Quantity = 400, Unit = Unit.Gram },
                new() { Name = "Coconut Milk", Quantity = 400, Unit = Unit.Milliliter },
                new() { Name = "Crushed Tomatoes", Quantity = 400, Unit = Unit.Gram },
                new() { Name = "Onion", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Garlic", Quantity = 4, Unit = Unit.Piece },
                new() { Name = "Ginger", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Curry Powder", Quantity = 2, Unit = Unit.Tablespoon },
                new() { Name = "Spinach", Quantity = 100, Unit = Unit.Gram, IsOptional = true },
                new() { Name = "Vegetable Oil", Quantity = 2, Unit = Unit.Tablespoon },
                new() { Name = "Salt", Quantity = 1, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Finely chop onion, garlic, and ginger.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Heat oil in a large pot. Sauté onion until soft, then add garlic and ginger; cook 1 minute.", DurationMinutes = 8 },
                new() { StepNumber = 3, Instruction = "Stir in curry powder and toast for 30 seconds.", DurationMinutes = 1 },
                new() { StepNumber = 4, Instruction = "Add crushed tomatoes and simmer 5 minutes.", DurationMinutes = 5 },
                new() { StepNumber = 5, Instruction = "Add chickpeas and coconut milk. Simmer 15 minutes until sauce thickens.", DurationMinutes = 15 },
                new() { StepNumber = 6, Instruction = "Stir in spinach until wilted. Season with salt. Serve with rice or naan.", DurationMinutes = 2 }
            ],
            Tags = ["Indian", "Vegan", "Healthy", "Dinner"],
            DietaryLabels = ["Vegan", "Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 30,
            Servings = 4
        },
        new()
        {
            Title = "Scrambled Eggs on Toast",
            Description = "Creamy, buttery scrambled eggs on toasted bread — the perfect quick breakfast.",
            Ingredients =
            [
                new() { Name = "Eggs", Quantity = 3, Unit = Unit.Piece },
                new() { Name = "Butter", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Bread", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Salt", Quantity = 0.5m, Unit = Unit.Teaspoon },
                new() { Name = "Black Pepper", Quantity = 0.25m, Unit = Unit.Teaspoon },
                new() { Name = "Chives", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Crack eggs into a bowl, season with salt and pepper, and beat until combined.", DurationMinutes = 1 },
                new() { StepNumber = 2, Instruction = "Melt butter in a non-stick pan over low-medium heat.", DurationMinutes = 1 },
                new() { StepNumber = 3, Instruction = "Pour in eggs. Stir slowly and continuously with a spatula, pulling from the edges. Remove from heat while still slightly glossy.", DurationMinutes = 4 },
                new() { StepNumber = 4, Instruction = "Toast the bread. Serve eggs on toast, garnished with chives.", DurationMinutes = 2 }
            ],
            Tags = ["Breakfast", "Quick", "Vegetarian"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 2,
            CookTimeMinutes = 5,
            Servings = 1
        },
        new()
        {
            Title = "Tomato Basil Soup",
            Description = "Velvety roasted tomato soup with fresh basil — simple, warming, and deeply flavourful.",
            Ingredients =
            [
                new() { Name = "Tomatoes", Quantity = 800, Unit = Unit.Gram },
                new() { Name = "Onion", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Garlic", Quantity = 4, Unit = Unit.Piece },
                new() { Name = "Vegetable Stock", Quantity = 500, Unit = Unit.Milliliter },
                new() { Name = "Olive Oil", Quantity = 3, Unit = Unit.Tablespoon },
                new() { Name = "Fresh Basil", Quantity = 20, Unit = Unit.Gram },
                new() { Name = "Salt", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Black Pepper", Quantity = 0.5m, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Halve tomatoes and place on a baking tray with garlic and onion. Drizzle with olive oil and roast at 200 °C for 25 minutes.", DurationMinutes = 25 },
                new() { StepNumber = 2, Instruction = "Transfer roasted vegetables to a pot, add vegetable stock and bring to a simmer.", DurationMinutes = 5 },
                new() { StepNumber = 3, Instruction = "Blend until smooth with an immersion blender.", DurationMinutes = 3 },
                new() { StepNumber = 4, Instruction = "Stir in fresh basil, season to taste, and serve hot.", DurationMinutes = 2 }
            ],
            Tags = ["Soup", "Vegan", "Comfort Food", "Lunch"],
            DietaryLabels = ["Vegan", "Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 30,
            Servings = 4
        },
        new()
        {
            Title = "Greek Salad",
            Description = "Refreshing Mediterranean salad with tomatoes, cucumber, olives, and feta cheese.",
            Ingredients =
            [
                new() { Name = "Tomatoes", Quantity = 3, Unit = Unit.Piece },
                new() { Name = "Cucumber", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Red Onion", Quantity = 0.5m, Unit = Unit.Piece },
                new() { Name = "Kalamata Olives", Quantity = 80, Unit = Unit.Gram },
                new() { Name = "Feta Cheese", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Olive Oil", Quantity = 3, Unit = Unit.Tablespoon },
                new() { Name = "Dried Oregano", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Salt", Quantity = 0.5m, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Chop tomatoes and cucumber into chunks. Thinly slice red onion.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Combine vegetables and olives in a bowl.", DurationMinutes = 1 },
                new() { StepNumber = 3, Instruction = "Crumble feta on top. Drizzle with olive oil, sprinkle with oregano and salt. Serve immediately.", DurationMinutes = 2 }
            ],
            Tags = ["Mediterranean", "Salad", "Quick", "Lunch"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 0,
            Servings = 2
        },
        new()
        {
            Title = "Pan-Seared Salmon with Lemon Butter",
            Description = "Crispy-skinned salmon fillet finished with a bright lemon-butter sauce — elegant and ready in minutes.",
            Ingredients =
            [
                new() { Name = "Salmon Fillet", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Butter", Quantity = 2, Unit = Unit.Tablespoon },
                new() { Name = "Lemon", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Garlic", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Olive Oil", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Salt", Quantity = 0.5m, Unit = Unit.Teaspoon },
                new() { Name = "Black Pepper", Quantity = 0.25m, Unit = Unit.Teaspoon },
                new() { Name = "Fresh Parsley", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Pat salmon dry with paper towels. Season both sides with salt and pepper.", DurationMinutes = 2 },
                new() { StepNumber = 2, Instruction = "Heat olive oil in a skillet over medium-high heat until shimmering.", DurationMinutes = 2 },
                new() { StepNumber = 3, Instruction = "Place salmon skin-side down. Sear 4 minutes without moving until skin is crispy.", DurationMinutes = 4 },
                new() { StepNumber = 4, Instruction = "Flip salmon and cook 2–3 more minutes until cooked through.", DurationMinutes = 3 },
                new() { StepNumber = 5, Instruction = "Reduce heat, add butter and garlic. Baste salmon with the melted butter for 1 minute.", DurationMinutes = 1 },
                new() { StepNumber = 6, Instruction = "Squeeze lemon juice over, garnish with parsley and serve immediately.", DurationMinutes = 1 }
            ],
            Tags = ["Seafood", "Healthy", "Quick", "Dinner"],
            DietaryLabels = [],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 5,
            CookTimeMinutes = 15,
            Servings = 2
        },
        new()
        {
            Title = "Banana Pancakes",
            Description = "Fluffy, naturally sweet banana pancakes — a satisfying vegetarian breakfast the whole family loves.",
            Ingredients =
            [
                new() { Name = "Bananas", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Eggs", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Flour", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Milk", Quantity = 150, Unit = Unit.Milliliter },
                new() { Name = "Baking Powder", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Butter", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Salt", Quantity = 0.25m, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Mash bananas in a bowl until smooth.", DurationMinutes = 2 },
                new() { StepNumber = 2, Instruction = "Whisk in eggs and milk.", DurationMinutes = 1 },
                new() { StepNumber = 3, Instruction = "Fold in flour, baking powder, and salt until just combined (small lumps are fine).", DurationMinutes = 2 },
                new() { StepNumber = 4, Instruction = "Heat a non-stick pan over medium heat and melt a little butter.", DurationMinutes = 2 },
                new() { StepNumber = 5, Instruction = "Pour small ladlefuls of batter into the pan. Cook until bubbles form (~2 min), then flip and cook 1 more minute.", DurationMinutes = 10 }
            ],
            Tags = ["Breakfast", "Vegetarian", "Sweet"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 5,
            CookTimeMinutes = 15,
            Servings = 4
        },
        new()
        {
            Title = "Red Lentil Soup",
            Description = "Warming, protein-rich red lentil soup spiced with cumin and turmeric — naturally vegan and very filling.",
            Ingredients =
            [
                new() { Name = "Red Lentils", Quantity = 300, Unit = Unit.Gram },
                new() { Name = "Onion", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Garlic", Quantity = 3, Unit = Unit.Piece },
                new() { Name = "Crushed Tomatoes", Quantity = 400, Unit = Unit.Gram },
                new() { Name = "Vegetable Stock", Quantity = 1, Unit = Unit.Liter },
                new() { Name = "Cumin", Quantity = 1, Unit = Unit.Teaspoon },
                new() { Name = "Turmeric", Quantity = 0.5m, Unit = Unit.Teaspoon },
                new() { Name = "Olive Oil", Quantity = 2, Unit = Unit.Tablespoon },
                new() { Name = "Lemon Juice", Quantity = 2, Unit = Unit.Tablespoon },
                new() { Name = "Salt", Quantity = 1, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Rinse lentils thoroughly under cold water.", DurationMinutes = 2 },
                new() { StepNumber = 2, Instruction = "Sauté diced onion and garlic in olive oil until soft.", DurationMinutes = 5 },
                new() { StepNumber = 3, Instruction = "Add cumin and turmeric; stir 1 minute.", DurationMinutes = 1 },
                new() { StepNumber = 4, Instruction = "Add lentils, crushed tomatoes, and vegetable stock. Bring to a boil.", DurationMinutes = 5 },
                new() { StepNumber = 5, Instruction = "Reduce heat and simmer 20 minutes until lentils are completely soft.", DurationMinutes = 20 },
                new() { StepNumber = 6, Instruction = "Partially blend to a creamy texture. Stir in lemon juice and season with salt.", DurationMinutes = 3 }
            ],
            Tags = ["Soup", "Vegan", "Healthy", "Dinner"],
            DietaryLabels = ["Vegan", "Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 30,
            Servings = 6
        },
        new()
        {
            Title = "Margherita Pizza",
            Description = "Classic Neapolitan-style pizza with a thin crust, fresh tomato sauce, mozzarella, and basil.",
            Ingredients =
            [
                new() { Name = "Pizza Dough", Quantity = 250, Unit = Unit.Gram },
                new() { Name = "Crushed Tomatoes", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Mozzarella Cheese", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Fresh Basil", Quantity = 10, Unit = Unit.Gram },
                new() { Name = "Olive Oil", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Garlic", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Salt", Quantity = 0.5m, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Preheat oven to 240 °C with a baking sheet or pizza stone inside.", DurationMinutes = 15 },
                new() { StepNumber = 2, Instruction = "Mix crushed tomatoes with minced garlic, olive oil, and salt to make the sauce.", DurationMinutes = 3 },
                new() { StepNumber = 3, Instruction = "Stretch dough on a floured surface into a thin round.", DurationMinutes = 5 },
                new() { StepNumber = 4, Instruction = "Spread tomato sauce over dough, then distribute torn mozzarella evenly.", DurationMinutes = 2 },
                new() { StepNumber = 5, Instruction = "Slide onto the hot baking sheet and bake 10–12 minutes until crust is golden and cheese is bubbling.", DurationMinutes = 12 },
                new() { StepNumber = 6, Instruction = "Top with fresh basil leaves and a drizzle of olive oil. Slice and serve.", DurationMinutes = 1 }
            ],
            Tags = ["Italian", "Vegetarian", "Dinner"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 20,
            CookTimeMinutes = 12,
            Servings = 2
        },
        new()
        {
            Title = "Chicken Noodle Soup",
            Description = "Soul-warming classic chicken soup with egg noodles, carrots, and celery.",
            Ingredients =
            [
                new() { Name = "Chicken Breast", Quantity = 400, Unit = Unit.Gram },
                new() { Name = "Egg Noodles", Quantity = 150, Unit = Unit.Gram },
                new() { Name = "Carrots", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Celery", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Onion", Quantity = 1, Unit = Unit.Piece },
                new() { Name = "Garlic", Quantity = 2, Unit = Unit.Piece },
                new() { Name = "Chicken Stock", Quantity = 1500, Unit = Unit.Milliliter },
                new() { Name = "Olive Oil", Quantity = 1, Unit = Unit.Tablespoon },
                new() { Name = "Fresh Parsley", Quantity = 10, Unit = Unit.Gram, IsOptional = true },
                new() { Name = "Salt", Quantity = 1, Unit = Unit.Teaspoon }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Dice onion, carrots, and celery; mince garlic.", DurationMinutes = 5 },
                new() { StepNumber = 2, Instruction = "Heat olive oil in a large pot; sauté onion, carrots, celery, and garlic until softened.", DurationMinutes = 8 },
                new() { StepNumber = 3, Instruction = "Add whole chicken breast and chicken stock. Bring to a boil, then reduce to a simmer.", DurationMinutes = 5 },
                new() { StepNumber = 4, Instruction = "Simmer 15 minutes until chicken is cooked through. Remove, shred with forks, and return to pot.", DurationMinutes = 15 },
                new() { StepNumber = 5, Instruction = "Bring soup back to a boil, add egg noodles, and cook per package directions.", DurationMinutes = 8 },
                new() { StepNumber = 6, Instruction = "Season with salt, stir in chopped parsley, and serve hot.", DurationMinutes = 2 }
            ],
            Tags = ["Soup", "Comfort Food", "Dinner", "Lunch"],
            DietaryLabels = [],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 15,
            CookTimeMinutes = 35,
            Servings = 6
        },
        new()
        {
            Title = "Overnight Oats",
            Description = "No-cook creamy oats prepared the night before — a healthy, customisable grab-and-go breakfast.",
            Ingredients =
            [
                new() { Name = "Rolled Oats", Quantity = 80, Unit = Unit.Gram },
                new() { Name = "Milk", Quantity = 150, Unit = Unit.Milliliter },
                new() { Name = "Greek Yogurt", Quantity = 80, Unit = Unit.Gram },
                new() { Name = "Honey", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true },
                new() { Name = "Bananas", Quantity = 1, Unit = Unit.Piece, IsOptional = true },
                new() { Name = "Chia Seeds", Quantity = 1, Unit = Unit.Tablespoon, IsOptional = true }
            ],
            Steps =
            [
                new() { StepNumber = 1, Instruction = "Combine oats, milk, Greek yogurt, and chia seeds in a jar or container. Stir well.", DurationMinutes = 3 },
                new() { StepNumber = 2, Instruction = "Drizzle in honey and stir again.", DurationMinutes = 1 },
                new() { StepNumber = 3, Instruction = "Cover and refrigerate overnight (at least 6 hours).", DurationMinutes = 0 },
                new() { StepNumber = 4, Instruction = "In the morning, top with sliced banana or your favourite fruit and serve cold.", DurationMinutes = 1 }
            ],
            Tags = ["Breakfast", "Healthy", "No-Cook"],
            DietaryLabels = ["Vegetarian"],
            Source = RecipeSource.Internal,
            PrepTimeMinutes = 5,
            CookTimeMinutes = 0,
            Servings = 1
        }
    ];
}
