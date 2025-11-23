using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CacheDemo.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a connection string directly for design-time
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=productcatalog;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("CacheDemo.Infrastructure"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}