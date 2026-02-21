using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure;
using PantryCloud.Notification.Infrastructure.Hubs;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.Notification.Presentation.Extensions;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogLogging(builder.Configuration);
builder.Host.UseSerilog();

builder.Services
    .AddPresentationLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApplicationLayerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing") && !string.IsNullOrEmpty(app.Configuration.GetConnectionString("DefaultConnection")))
{
    await app.ApplyMigrationsAsync<NotificationDbContext>();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

