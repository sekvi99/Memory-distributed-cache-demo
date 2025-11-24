using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheDemo.Application.Commands.Handlers;

public class UpdateProductCommandHandler(
    IProductRepository repository,
    IDistributedCache distributedCache)
    : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            return Result<ProductDto>.Failure("Product not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await repository.UpdateAsync(product, cancellationToken);

        // Invalidate caches
        await distributedCache.RemoveAsync("all-products", cancellationToken);
        await distributedCache.RemoveAsync($"product-{request.Id}", cancellationToken);

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