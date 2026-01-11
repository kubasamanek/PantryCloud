using PantryCloud.ApiGateway.Infrastructure;
using PantryCloud.ApiGateway.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentationLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration);

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

app.UseExceptionHandler();

app.MapReverseProxy();

app.MapControllers();

app.Run();

