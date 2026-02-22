using PantryCloud.Pantry.Application;
using PantryCloud.Pantry.Infrastructure;
using PantryCloud.Pantry.Infrastructure.Persistence;
using PantryCloud.Pantry.Presentation.Extensions;
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

await app.ApplyMigrationsAsync<PantryDbContext>();

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
