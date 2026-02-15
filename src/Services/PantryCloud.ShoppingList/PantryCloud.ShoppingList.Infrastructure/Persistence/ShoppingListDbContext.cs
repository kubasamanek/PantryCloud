using Microsoft.EntityFrameworkCore;
using PantryCloud.ShoppingList.Core.Configurations;
using PantryCloud.ShoppingList.Core.Entities;

namespace PantryCloud.ShoppingList.Infrastructure.Persistence;

public class ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : DbContext(options)
{
    public DbSet<Core.Entities.ShoppingList> ShoppingLists => Set<Core.Entities.ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ShoppingListConfiguration).Assembly);

        base.OnModelCreating(builder);
    }
}


