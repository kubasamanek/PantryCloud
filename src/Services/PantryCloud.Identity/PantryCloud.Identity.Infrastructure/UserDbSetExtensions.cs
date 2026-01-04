using Microsoft.EntityFrameworkCore;
using PantryCloud.Identity.Core.Entities;

namespace PantryCloud.Identity.Infrastructure;

internal static class UserDbSetExtensions
{
    public static async Task<bool> Exists(this DbSet<ApplicationUser> users, string email)
    {
        return await users.AnyAsync(u => u.Email == email);
    }

    public static async Task<ApplicationUser?> GetByEmail(this DbSet<ApplicationUser> users, string email)
    {
        return await users.SingleOrDefaultAsync(u => u.Email == email);
    }
}
