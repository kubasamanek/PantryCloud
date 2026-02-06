using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.ShoppingList.UnitTests;

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

    public static class ShoppingList
    {
        public static readonly Guid Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public const string Name = "Test Shopping List";
        public const string DuplicateName = "Duplicate List";
    }

    public static class ShoppingListItem
    {
        public static readonly Guid Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        public const string Name = "Milk";
        public const decimal Quantity = 2m;
        public static readonly Unit Unit = Unit.Liter;
    }

    public static class RowVersion
    {
        public static readonly byte[] Default = [1, 2, 3, 4];
        public static readonly byte[] Mismatched = [9, 9, 9, 9];
    }

    public static class Pagination
    {
        public const int Page1 = 1;
        public const int PageSize50 = 50;
    }

    public static class Dates
    {
        public static readonly DateTime ThirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        public static readonly DateTime TenDaysAgo = DateTime.UtcNow.AddDays(-10);
        public static readonly DateTime FiveDaysAgo = DateTime.UtcNow.AddDays(-5);
    }

    public static class Errors
    {
        public const string HouseholdNotFound = "ShoppingList.HouseholdNotFound";
        public const string ShoppingListNotFound = "ShoppingList.ListNotFound";
        public const string ItemNotFound = "ShoppingList.ItemNotFound";
        public const string DuplicateListName = "ShoppingList.DuplicateListName";
        public const string ConcurrencyConflict = "ShoppingList.ConcurrencyConflict";
        public const string UnauthorizedAccess = "ShoppingList.UnauthorizedAccess";
    }

    public static readonly Guid NonExistentListId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NonExistentItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");
}
