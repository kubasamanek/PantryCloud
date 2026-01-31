namespace PantryCloud.Recipe.Core.Entities;

public class RecipeStep
{
    public int StepNumber { get; set; }
    public required string Instruction { get; set; }
    public int? DurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
}


