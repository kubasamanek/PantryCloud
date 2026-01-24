using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using PantryCloud.SharedKernel.Identity;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Identity;

public class UserContextTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserContext _sut;

    public UserContextTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new UserContext(_httpContextAccessor);
    }

    [Fact]
    public void UserId_ShouldReturnUserId_WhenClaimTypeNameIdentifier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.ShouldBe(userId);
    }

    [Fact]
    public void UserId_ShouldReturnUserId_WhenJwtSubClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString())
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.ShouldBe(userId);
    }

    [Fact]
    public void UserId_ShouldThrowUnauthorizedException_WhenUserIdNotFound()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "test@example.com")
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.UserId)
            .Message.ShouldBe("User ID not found in token");
    }

    [Fact]
    public void UserId_ShouldThrowUnauthorizedException_WhenUserIdIsNotValidGuid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "not-a-guid")
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.UserId)
            .Message.ShouldBe("User ID not found in token");
    }

    [Fact]
    public void Email_ShouldReturnEmail_WhenClaimTypeEmail()
    {
        // Arrange
        var email = "test@example.com";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, email)
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.Email;

        // Assert
        result.ShouldBe(email);
    }

    [Fact]
    public void Email_ShouldReturnEmail_WhenJwtEmailClaim()
    {
        // Arrange
        var email = "jwt@example.com";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, email)
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.Email;

        // Assert
        result.ShouldBe(email);
    }

    [Fact]
    public void Email_ShouldThrowUnauthorizedException_WhenEmailNotFound()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var httpContext = CreateHttpContext(claims);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.Email)
            .Message.ShouldBe("Email not found in token");
    }

    [Fact]
    public void UserId_ShouldThrowUnauthorizedException_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.UserId)
            .Message.ShouldBe("No HttpContext found");
    }

    [Fact]
    public void UserId_ShouldThrowUnauthorizedException_WhenUserIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = null!;
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.UserId)
            .Message.ShouldBe("User not authenticated");
    }

    [Fact]
    public void UserId_ShouldThrowUnauthorizedException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated (no authentication type)
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act & Assert
        Should.Throw<UnauthorizedAccessException>(() => _sut.UserId)
            .Message.ShouldBe("User not authenticated");
    }

    private static DefaultHttpContext CreateHttpContext(List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };
        return httpContext;
    }
}

