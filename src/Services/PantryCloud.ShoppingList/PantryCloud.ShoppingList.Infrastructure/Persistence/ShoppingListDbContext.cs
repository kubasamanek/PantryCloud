using Microsoft.EntityFrameworkCore;
using PantryCloud.ShoppingList.Core.Configurations;
using PantryCloud.ShoppingList.Core.Entities;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.ShoppingList.Infrastructure.Persistence;

public class ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : DbContext(options)
{
    public DbSet<Core.Entities.ShoppingList> ShoppingLists => Set<Core.Entities.ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ShoppingListConfiguration).Assembly);
        builder.ApplyConfiguration(new OutboxMessageConfiguration());
        builder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());

        base.OnModelCreating(builder);
    }
}
