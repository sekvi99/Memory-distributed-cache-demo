using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CacheDemo.Application.Queries.Handlers;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository, IMemoryCache memoryCache)
    {
        _repository = repository;
        _memoryCache = memoryCache;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product-{request.Id}";

        // Try to get from memory cache first
        if (_memoryCache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
            return Result<ProductDto>.Success(cachedProduct);

        // Fetch from database
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

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

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _memoryCache.Set(cacheKey, productDto, cacheOptions);

        return Result<ProductDto>.Success(productDto);
        ;
    }
}