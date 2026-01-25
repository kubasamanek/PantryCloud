using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.Pantry.UnitTests;

internal static class Constants
{
    // User constants
    public static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public const string UserEmail = "user@example.com";
    
    // Household constants
    public static readonly Guid HouseholdId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid OldHouseholdId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    
    // Pantry item constants
    public static readonly Guid PantryItemId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public const string PantryItemName = "Test Item";
    public const decimal PantryItemQuantity = 1.5m;
    public static readonly Unit PantryItemUnit = Unit.Kilogram;
    public static readonly DateTime? PantryItemExpirationDate = DateTime.UtcNow.AddDays(7);
    public const string PantryItemCategory = "Fruits";
    public const string PantryItemNotes = "Test notes";
    public const string PantryItemImageUrl = "https://example.com/image.jpg";
    
    // Test data constants for list operations
    public const string TestItem1Name = "Item 1";
    public const string TestItem2Name = "Item 2";
    public const decimal TestItem1Quantity = 1m;
    public const decimal TestItem2Quantity = 2m;
    public static readonly Unit TestItem1Unit = Unit.Piece;
    public static readonly Unit TestItem2Unit = Unit.Kilogram;
    
    // Test data constants for filtering
    public const string TestAppleName = "Apple";
    public const string TestBananaName = "Banana";
    public const string TestBreadName = "Bread";
    public const string TestFruitsCategory = "Fruits";
    public const string TestBakeryCategory = "Bakery";
    
    // Test data constants for update operations
    public const string UpdatedItemName = "Updated Name";
    public const decimal UpdatedItemQuantity = 2.5m;
    public static readonly Unit UpdatedItemUnit = Unit.Liter;
    public const string UpdatedItemCategory = "Updated Category";
    public const string UpdatedItemNotes = "Updated Notes";
    public const string UpdatedItemImageUrl = "https://example.com/updated.jpg";
    public static readonly DateTime UpdatedItemExpirationDate = DateTime.UtcNow.AddDays(14);
    
    // Row version constants
    public static readonly byte[] DefaultRowVersion = [1, 2, 3, 4];
    public static readonly byte[] TestRowVersion1 = [1];
    public static readonly byte[] TestRowVersion2 = [2];
    
    // Date constants
    public static readonly DateTime ThirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
    public static readonly DateTime TenDaysAgo = DateTime.UtcNow.AddDays(-10);
    public static readonly DateTime FiveDaysAgo = DateTime.UtcNow.AddDays(-5);
}
