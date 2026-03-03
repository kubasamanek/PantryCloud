using Microsoft.EntityFrameworkCore;
using PantryCloud.Household.Core.Configurations;
using PantryCloud.Household.Core.Entities;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.Household.Infrastructure.Persistence;

public class HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : DbContext(options)
{
    public DbSet<Core.Entities.Household> Households => Set<Core.Entities.Household>();
    public DbSet<HouseholdMember> Members => Set<HouseholdMember>();
    public DbSet<HouseholdInvitation> Invitations => Set<HouseholdInvitation>();
    public DbSet<MemberPreference> MemberPreferences => Set<MemberPreference>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(HouseholdConfiguration).Assembly);
        builder.ApplyConfiguration(new OutboxMessageConfiguration());
        builder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());

        base.OnModelCreating(builder);
    }
}