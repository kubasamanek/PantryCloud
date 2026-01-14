using PantryCloud.Identity.Application;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.Presentation.Extensions;
using PantryCloud.SharedKernel.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentationLayerServices()
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApplicationLayerServices();

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

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();