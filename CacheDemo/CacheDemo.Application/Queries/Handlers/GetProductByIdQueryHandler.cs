using System.Text.Json;
using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheDemo.Application.Queries.Handlers;

public class GetProductByIdQueryHandler(IProductRepository repository, IDistributedCache distributedCache)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product-{request.Id}";

        // Try to get from cache first
        var cachedData = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedProduct = JsonSerializer.Deserialize<ProductDto>(cachedData);
            if (cachedProduct != null)
                return Result<ProductDto>.Success(cachedProduct);
        }

        // Fetch from database
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            return Result<ProductDto>.Failure("Product not found");

        var productDto = new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt
        );

        // Cache in Redis for 10 minutes
        var serializedData = JsonSerializer.Serialize(productDto);
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        await distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

        return Result<ProductDto>.Success(productDto);
    }
}