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

public class GetAllProductsQueryTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly GetAllProductsQueryHandler _handler;
    private readonly Mock<IProductRepository> _repositoryMock;

    public GetAllProductsQueryTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _distributedCacheMock = new Mock<IDistributedCache>();

        _handler = new GetAllProductsQueryHandler(
            _repositoryMock.Object,
            _distributedCacheMock.Object
        );
    }

    [Fact]
    public async Task Handle_CacheMiss_ReturnsFromRepositoryAndSetsCache()
    {
        // Arrange
        var productList = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "FromRepo",
                Description = "D",
                Price = 10m,
                Stock = 20,
                CreatedAt = DateTime.UtcNow
            }
        };

        _distributedCacheMock
            .Setup(c => c.GetAsync("all-products", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null); // Cache miss

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(productList);

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("FromRepo", result.Data![0].Name);

        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);

        _distributedCacheMock.Verify(c =>
                c.SetAsync(
                    "all-products",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }


    [Fact]
    public async Task Handle_UsesCachedValue_ReturnsCachedDtos()
    {
        // Arrange
        var dtoList = new List<ProductDto>
        {
            new(Guid.NewGuid(), "Cached", "D", 2m, 2, DateTime.UtcNow, null)
        };

        var json = JsonSerializer.Serialize(dtoList);
        var bytes = Encoding.UTF8.GetBytes(json);

        _distributedCacheMock
            .Setup(c => c.GetAsync("all-products", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Cached", result.Data![0].Name);

        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}