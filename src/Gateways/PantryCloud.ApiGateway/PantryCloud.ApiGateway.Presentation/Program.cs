using PantryCloud.ApiGateway.Core;
using PantryCloud.ApiGateway.Infrastructure;
using PantryCloud.ApiGateway.Infrastructure.Middleware;
using PantryCloud.ApiGateway.Presentation.Extensions;
using PantryCloud.SharedKernel.Correlation;

var builder = WebApplication.CreateBuilder(args);

var apiConfiguration = new ApiConfiguration();
builder.Configuration.Bind(apiConfiguration);

builder.Services
    .AddPresentationLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddRateLimiting(apiConfiguration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (apiConfiguration.Gateway.RateLimit.Enabled)
{
    app.UseRateLimiter();
}

app.MapReverseProxy();

app.MapControllers();

app.Run();
