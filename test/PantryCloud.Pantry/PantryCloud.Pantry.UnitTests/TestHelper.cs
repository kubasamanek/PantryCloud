using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Pantry.UnitTests;

internal static class TestHelper
{
    public static PantryDbContext CreateInMemoryContext(string dbName, IUserContext? userContext = null)
    {
        var options = new DbContextOptionsBuilder<PantryDbContext>()
            .UseInMemoryDatabase(dbName + "_" + Guid.NewGuid())
            .Options;
        return new TestPantryDbContext(options, userContext);
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

