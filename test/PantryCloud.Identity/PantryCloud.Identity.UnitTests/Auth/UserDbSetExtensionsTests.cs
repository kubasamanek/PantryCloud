using Microsoft.EntityFrameworkCore;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.UnitTests.Auth;

public class UserDbSetExtensionsTests
{
    [Fact]
    public async Task Exists_ReturnsTrue_WhenEmailExists()
    {
        await using var ctx = TestHelper.CreateInMemoryContext(nameof(Exists_ReturnsTrue_WhenEmailExists));
        ctx.Users.Add(Constants.ExampleUser);
        await ctx.SaveChangesAsync();

        var exists = await ctx.Users.Exists(Constants.User.Email);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Exists_ReturnsFalse_WhenEmailMissing()
    {
        await using var ctx = TestHelper.CreateInMemoryContext(nameof(Exists_ReturnsFalse_WhenEmailMissing));

        var exists = await ctx.Users.Exists(Constants.Emails.NotFound);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByEmail_ReturnsUser_WhenEmailMatches()
    {
        await using var ctx = TestHelper.CreateInMemoryContext(nameof(GetByEmail_ReturnsUser_WhenEmailMatches));
        ctx.Users.Add(Constants.ExampleUser);
        await ctx.SaveChangesAsync();

        var result = await ctx.Users.GetByEmail(Constants.User.Email);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(Constants.User.Id);
        result.Email.ShouldBe(Constants.User.Email);
    }

    [Fact]
    public async Task GetByEmail_ReturnsNull_WhenNotFound()
    {
        await using var ctx = TestHelper.CreateInMemoryContext(nameof(GetByEmail_ReturnsNull_WhenNotFound));

        var result = await ctx.Users.GetByEmail(Constants.Emails.Missing);

        result.ShouldBeNull();
    }
}