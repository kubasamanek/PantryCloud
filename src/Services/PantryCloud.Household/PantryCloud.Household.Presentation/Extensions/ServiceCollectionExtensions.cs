using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using PantryCloud.Household.Presentation.Exceptions;

namespace PantryCloud.Household.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGenWithAuth();

        services.AddEndpointsApiExplorer();
        
        services.AddControllers();
        
        services.AddAutoMapper(_ => {}, typeof(Program));
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }

    private static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
    
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };
    
            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    
            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    []
                }
            };
    
            o.AddSecurityRequirement(securityRequirement);
        });
    
        return services;
    }
    
}