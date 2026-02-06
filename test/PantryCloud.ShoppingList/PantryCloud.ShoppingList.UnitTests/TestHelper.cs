using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.ShoppingList.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.ShoppingList.UnitTests;

internal static class TestHelper
{
    public static ShoppingListDbContext CreateInMemoryContext(string dbName, IUserContext? userContext = null)
    {
        var options = new DbContextOptionsBuilder<ShoppingListDbContext>()
            .UseInMemoryDatabase(dbName + "_" + Guid.NewGuid())
            .Options;
        return new TestShoppingListDbContext(options, userContext);
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
        var context = new DefaultHttpContext { User = claimsPrincipal };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return new UserContext(accessor);
    }
}
