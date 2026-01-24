using PantryCloud.Identity.Application;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.Presentation.Extensions;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentationLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApplicationLayerServices();

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    await app.ApplyMigrationsAsync<ApplicationDbContext>();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();