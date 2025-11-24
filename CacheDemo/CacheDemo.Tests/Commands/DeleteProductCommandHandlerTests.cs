using CacheDemo.Application.Commands;
using CacheDemo.Application.Commands.Handlers;
using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CacheDemo.Tests.Commands;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly DeleteProductCommandHandler _handler;
    private readonly Mock<IProductRepository> _repositoryMock;

    public DeleteProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _distributedCacheMock = new Mock<IDistributedCache>();

        _handler = new DeleteProductCommandHandler(
            _repositoryMock.Object,
            _distributedCacheMock.Object
        );
    }

    [Fact]
    public async Task Handle_ProductExists_DeletesAndInvalidatesCache()
    {
        var productId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId });

        _repositoryMock
            .Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _distributedCacheMock
            .Setup(c => c.RemoveAsync("all-products", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _distributedCacheMock
            .Setup(c => c.RemoveAsync($"product-{productId}", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteProductCommand(productId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        _repositoryMock.Verify(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _distributedCacheMock.Verify(c => c.RemoveAsync("all-products", It.IsAny<CancellationToken>()), Times.Once);
        _distributedCacheMock.Verify(c => c.RemoveAsync($"product-{productId}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductDoesNotExist_ReturnsFailure()
    {
        var productId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new DeleteProductCommand(productId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
    }
}