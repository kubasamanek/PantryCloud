using PantryCloud.Notification.Application;
using PantryCloud.Notification.Infrastructure;
using PantryCloud.Notification.Infrastructure.Hubs;
using PantryCloud.Notification.Presentation.Extensions;


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
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

app.MapHealthChecks("/health");

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

