using System.Text.Json;
using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheDemo.Application.Queries.Handlers;

public class GetAllProductsQueryHandler(IProductRepository repository, IDistributedCache distributedCache)
    : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
{
    public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "all-products";

        // Try to get from distributed cache (Redis)
        var cachedData = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedProducts = JsonSerializer.Deserialize<List<ProductDto>>(cachedData);
            if (cachedProducts != null)
                return Result<List<ProductDto>>.Success(cachedProducts);
        }

        // Fetch from database
        var products = await repository.GetAllAsync(cancellationToken);

        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();

        // Cache in Redis for 10 minutes
        var serializedData = JsonSerializer.Serialize(productDtos);
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        await distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

        return Result<List<ProductDto>>.Success(productDtos);
        ;
    }
}