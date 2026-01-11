using PantryCloud.Household.Application;
using PantryCloud.Household.Infrastructure;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.Presentation.Extensions;
using PantryCloud.SharedKernel.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentationLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApplicationLayerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    await app.ApplyMigrationsAsync<HouseholdDbContext>();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

app.MapControllers();

app.Run();