using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;
using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheDemo.Application.Commands.Handlers;

public class CreateProductCommandHandler(
    IProductRepository repository,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache)
    : IRequestHandler<CreateProductCommand,
        Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(product, cancellationToken);

        // Invalidate caches
        memoryCache.Remove("all-products");
        await distributedCache.RemoveAsync("all-products", cancellationToken);

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt)
        );
    }
}