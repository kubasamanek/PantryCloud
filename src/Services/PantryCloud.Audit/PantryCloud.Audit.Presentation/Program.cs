using PantryCloud.Audit.Application;
using PantryCloud.Audit.Infrastructure;
using PantryCloud.Audit.Presentation.Extensions;
using PantryCloud.Audit.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Logging;
using PantryCloud.SharedKernel.Observability;
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

await app.ApplyMigrationsAsync<AuditDbContext>();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusMetrics();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
