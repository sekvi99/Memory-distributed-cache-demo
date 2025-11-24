using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheDemo.Application.Commands.Handlers;

public class DeleteProductCommandHandler(
    IProductRepository repository,
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
        await distributedCache.RemoveAsync("all-products", cancellationToken);
        await distributedCache.RemoveAsync($"product-{request.Id}", cancellationToken);

        return Result<bool>.Success(true);
    }
}