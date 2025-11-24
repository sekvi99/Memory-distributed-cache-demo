using System.Text;
using System.Text.Json;
using CacheDemo.Application.DTOs;
using CacheDemo.Application.Queries;
using CacheDemo.Application.Queries.Handlers;
using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CacheDemo.Tests.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly GetProductByIdQueryHandler _handler;
    private readonly Mock<IProductRepository> _repositoryMock;

    public GetProductByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _distributedCacheMock = new Mock<IDistributedCache>();
        _handler = new GetProductByIdQueryHandler(_repositoryMock.Object, _distributedCacheMock.Object);
    }

    [Fact]
    public async Task Handle_ProductExists_ReturnsProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cacheKey = $"product-{productId}";

        // Cache miss → return null bytes
        _distributedCacheMock
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(productId, result.Data.Id);
    }

    [Fact]
    public async Task Handle_ProductDoesNotExist_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var cacheKey = $"product-{productId}";

        _distributedCacheMock
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var query = new GetProductByIdQuery(productId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
    }

    [Fact]
    public async Task Handle_UsesCachedValue_ReturnsCachedDto()
    {
        var productId = Guid.NewGuid();
        var cacheKey = $"product-{productId}";

        var dto = new ProductDto(
            productId,
            "Cached Product",
            "Description",
            49.99m,
            5,
            DateTime.UtcNow,
            null
        );

        var json = JsonSerializer.Serialize(dto);
        var bytes = Encoding.UTF8.GetBytes(json);

        _distributedCacheMock
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var query = new GetProductByIdQuery(productId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Cached Product", result.Data!.Name);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}