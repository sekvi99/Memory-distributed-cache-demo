using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheDemo.Application.Commands.Handlers;

public class DeleteProductCommandHandler(
    IProductRepository repository,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache)
    : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            return Result<bool>.Failure("Product not found");

        await repository.DeleteAsync(request.Id, cancellationToken);

        // Invalidate caches
        memoryCache.Remove($"product-{request.Id}");
        memoryCache.Remove("all-products");
        await distributedCache.RemoveAsync("all-products", cancellationToken);

        return Result<bool>.Success(true);
    }
}