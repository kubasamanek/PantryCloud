using PantryCloud.Identity.Application;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Presentation.Extensions;

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
    
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();