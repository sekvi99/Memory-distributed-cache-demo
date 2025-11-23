using Microsoft.OpenApi.Models;

namespace CacheDemo.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cache Demo API", Version = "v1" });
        });

        return services;
    }
} 