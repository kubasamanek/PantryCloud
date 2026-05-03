using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.Pantry.UnitTests;

internal static class Constants
{
    public static class User
    {
        public static readonly Guid Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public const string Email = "user@example.com";
    }

    public static class Household
    {
        public static readonly Guid Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid OldId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    }

    public static class PantryItem
    {
        public static readonly Guid Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public const string Name = "Test Item";
        public const decimal Quantity = 1.5m;
        public static readonly Unit Unit = Unit.Kilogram;
        public const string Category = "Fruits";
        public const string Notes = "Test notes";
        public const string ImageUrl = "https://example.com/image.jpg";
        public static readonly DateTime? ExpirationDate = DateTime.UtcNow.AddDays(7);
    }

    public static class UpdatedPantryItem
    {
        public const string Name = "Updated Name";
        public const decimal Quantity = 2.5m;
        public static readonly Unit Unit = Unit.Liter;
        public const string Category = "Updated Category";
        public const string Notes = "Updated Notes";
        public const string ImageUrl = "https://example.com/updated.jpg";
        public static readonly DateTime ExpirationDate = DateTime.UtcNow.AddDays(14);
    }

    public static class ListItems
    {
        public const string Item1Name = "Item 1";
        public const string Item2Name = "Item 2";
        public const decimal Item1Quantity = 1m;
        public const decimal Item2Quantity = 2m;
        public static readonly Unit Item1Unit = Unit.Piece;
        public static readonly Unit Item2Unit = Unit.Kilogram;
    }

    public static class FilterItems
    {
        public const string AppleName = "Apple";
        public const string BananaName = "Banana";
        public const string BreadName = "Bread";
        public const string FruitsCategory = "Fruits";
        public const string BakeryCategory = "Bakery";
    }

    public static class SearchCaseInsensitive
    {
        public const string MilkName = "Milk";
        public const string SearchLower = "milk";
        public const string SearchUpper = "MILK";
    }

    public static class ExpirationCheck
    {
        public const string MilkName = "Milk";
        public const string BreadName = "Bread";
        public const string EggsName = "Eggs";
        public const string NoExpiryName = "No expiry";
    }

    public static class RowVersion
    {
        public static readonly byte[] Default = [1, 2, 3, 4];
        public static readonly byte[] Mismatched = [9, 9, 9, 9];
        public static readonly byte[] Version1 = [1];
        public static readonly byte[] Version2 = [2];
    }

    public static class Dates
    {
        public static readonly DateTime ThirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        public static readonly DateTime TenDaysAgo = DateTime.UtcNow.AddDays(-10);
        public static readonly DateTime FiveDaysAgo = DateTime.UtcNow.AddDays(-5);
    }

    public static class Pagination
    {
        public const int Page1 = 1;
        public const int PageSize50 = 50;
    }

    public static readonly Guid NonExistentItemId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
}
