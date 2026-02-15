using Microsoft.EntityFrameworkCore;
using PantryCloud.Pantry.Core.Configurations;
using PantryCloud.Pantry.Core.Entities;

namespace PantryCloud.Pantry.Infrastructure.Persistence;

public class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options)
{
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PantryItemConfiguration).Assembly);

        base.OnModelCreating(builder);
    }
}

