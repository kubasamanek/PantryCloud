using PantryCloud.Recipe.Application;
using PantryCloud.Recipe.Infrastructure;
using PantryCloud.Recipe.Presentation.Extensions;
using PantryCloud.SharedKernel.Correlation;
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
