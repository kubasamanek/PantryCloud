using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PantryCloud.Pantry.Infrastructure.Services;

public class HouseholdCacheHydrationService(
    PantryDbContext dbContext,
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    ILogger<HouseholdCacheHydrationService> logger)
    : IHouseholdCacheHydrationService
{
    private readonly string _householdServiceUrl = configuration["Services:HouseholdService"] ?? "http://household-api:8080";

    public async Task<bool> HydrateCacheForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Hydrating cache for user {UserId}", userId);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_householdServiceUrl}/api/households/me");
            
            request.AddCorrelationIdHeader(httpContextAccessor);
            
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.ContainsKey("Authorization") == true)
            {
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            }

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to hydrate cache for user {UserId}: {StatusCode}", userId, response.StatusCode);
                return false;
            }

            var householdData = await response.Content.ReadFromJsonAsync<HouseholdResponse>(cancellationToken: cancellationToken);
            
            if (householdData == null)
            {
                logger.LogWarning("No household data returned for user {UserId}", userId);
                return false;
            }

            var existingMembership = await dbContext.UserHouseholdMemberships
                .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

            if (existingMembership != null)
            {
                if (existingMembership.HouseholdId != householdData.Id)
                {
                    existingMembership.LeftAt = DateTime.UtcNow;
                }
                else if (existingMembership.LeftAt != null)
                {
                    existingMembership.LeftAt = null;
                }
                existingMembership.HouseholdId = householdData.Id;
            }
            else
            {
                var membership = new UserHouseholdMembership
                {
                    UserId = userId,
                    HouseholdId = householdData.Id,
                    JoinedAt = DateTime.UtcNow
                };
                await dbContext.UserHouseholdMemberships.AddAsync(membership, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully hydrated cache for user {UserId}, HouseholdId: {HouseholdId}", userId, householdData.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error hydrating cache for user {UserId}", userId);
            return false;
        }
    }

    public Task<bool> HydrateAllCacheAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("HydrateAllCacheAsync is not implemented - would require listing all users from Identity service");
        return Task.FromResult(false);
    }

    private record HouseholdResponse(Guid Id, string Name);
}

