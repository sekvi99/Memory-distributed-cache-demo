using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using CacheDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CacheDemo.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}