using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PantryCloud.Recipe.Core.Enums;

namespace PantryCloud.Recipe.Core.Entities;

public class Recipe
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("ingredients")]
    public List<Ingredient> Ingredients { get; set; } = new();

    [BsonElement("steps")]
    public List<RecipeStep> Steps { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("dietaryLabels")]
    public List<string> DietaryLabels { get; set; } = new();

    [BsonElement("source")]
    [BsonRepresentation(BsonType.Int32)]
    public RecipeSource Source { get; set; } = RecipeSource.Internal;

    [BsonElement("prepTimeMinutes")]
    public int PrepTimeMinutes { get; set; }

    [BsonElement("cookTimeMinutes")]
    public int CookTimeMinutes { get; set; }

    [BsonElement("servings")]
    public int Servings { get; set; } = 4;

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


