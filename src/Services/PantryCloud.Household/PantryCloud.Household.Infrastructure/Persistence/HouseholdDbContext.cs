using Microsoft.EntityFrameworkCore;
using PantryCloud.Household.Core.Configurations;
using PantryCloud.Household.Core.Entities;

namespace PantryCloud.Household.Infrastructure.Persistence;

public class HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : DbContext(options)
{
    public DbSet<Core.Entities.Household> Households => Set<Core.Entities.Household>();
    public DbSet<HouseholdMember> Members => Set<HouseholdMember>();
    public DbSet<HouseholdInvitation> Invitations => Set<HouseholdInvitation>();
    public DbSet<Core.Entities.MemberPreference> MemberPreferences => Set<Core.Entities.MemberPreference>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(HouseholdConfiguration).Assembly);

        base.OnModelCreating(builder);
    }
}