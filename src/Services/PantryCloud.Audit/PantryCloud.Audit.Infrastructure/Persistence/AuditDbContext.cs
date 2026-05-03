using Microsoft.EntityFrameworkCore;
using PantryCloud.Audit.Core.Configurations;
using PantryCloud.Audit.Core.Entities;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.Audit.Infrastructure.Persistence;

public class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<HouseholdAuditEntry> HouseholdAuditEntries => Set<HouseholdAuditEntry>();
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(HouseholdAuditEntryConfiguration).Assembly);
        builder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());
        base.OnModelCreating(builder);
    }
}
