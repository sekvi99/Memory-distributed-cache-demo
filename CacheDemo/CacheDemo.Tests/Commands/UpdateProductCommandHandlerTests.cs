using CacheDemo.Application.Commands;
using CacheDemo.Application.Commands.Handlers;
using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CacheDemo.Tests.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly UpdateProductCommandHandler _handler;
    private readonly Mock<IProductRepository> _repositoryMock;

    public UpdateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _distributedCacheMock = new Mock<IDistributedCache>();

        _handler = new UpdateProductCommandHandler(
            _repositoryMock.Object,
            _distributedCacheMock.Object
        );
    }

    [Fact]
    public async Task Handle_ProductExists_UpdatesAndInvalidatesCache()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existing = new Product
        {
            Id = productId,
            Name = "Old",
            Description = "Old Desc",
            Price = 5m,
            Stock = 1,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _distributedCacheMock
            .Setup(c => c.RemoveAsync("all-products", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _distributedCacheMock
            .Setup(c => c.RemoveAsync($"product-{productId}", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateProductCommand(productId, "New", "New Desc", 9.99m, 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("New", result.Data!.Name);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
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

        var command = new UpdateProductCommand(productId, "New", "New Desc", 9.99m, 3);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
    }
}