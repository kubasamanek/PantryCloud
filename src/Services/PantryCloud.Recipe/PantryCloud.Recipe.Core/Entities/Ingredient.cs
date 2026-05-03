using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.Recipe.Core.Entities;

public class Ingredient
{
    public required string Name { get; set; }
    public decimal Quantity { get; set; }
    public Unit Unit { get; set; }
    public bool IsOptional { get; set; }
}

