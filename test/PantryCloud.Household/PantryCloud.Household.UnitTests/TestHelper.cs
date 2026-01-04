using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Household.Application;
using PantryCloud.Household.Core;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.Infrastructure.Services;

namespace PantryCloud.Household.UnitTests;

internal static class TestHelper
{
    public static HouseholdDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<HouseholdDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new HouseholdDbContext(options);
    }
    
    public static ApiConfiguration MockConfiguration()
    {
        var tp = new ApiConfiguration();
        return tp;
    }
    
    public static ILogger<T> MockLogger<T>() where T : class
        => Substitute.For<ILogger<T>>();
    
    public static IUserContext CreateMockUserContext(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };

        var identity = new ClaimsIdentity(claims, "mock");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        return new UserContext(accessor);
    }

}