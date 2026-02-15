using Microsoft.EntityFrameworkCore;
using PantryCloud.Identity.Core.Entities;

namespace PantryCloud.Identity.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users { get; set; }

    public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }

    public DbSet<VerifyEmailToken> VerifyEmailTokens { get; set; }
    public DbSet<RefreshSession> RefreshSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationUser).Assembly);

        base.OnModelCreating(builder);
    }
}