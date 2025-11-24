using CacheDemo.Domain.Interfaces;
using CacheDemo.Infrastructure.Data;
using CacheDemo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CacheDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("CacheDemo.Infrastructure")));

        // Redis Connection (with error handling)
        try
        {
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
        }
        catch
        {
        }

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}