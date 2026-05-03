using Microsoft.EntityFrameworkCore;
using PantryCloud.Pantry.Core.Configurations;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.Pantry.Infrastructure.Persistence;

public class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options)
{
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PantryItemConfiguration).Assembly);
        builder.ApplyConfiguration(new OutboxMessageConfiguration());
        builder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());

        base.OnModelCreating(builder);
    }
}
