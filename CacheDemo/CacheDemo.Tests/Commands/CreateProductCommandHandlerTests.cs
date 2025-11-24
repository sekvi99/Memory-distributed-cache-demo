using CacheDemo.Application.Commands;
using CacheDemo.Application.Commands.Handlers;
using CacheDemo.Domain.Entities;
using CacheDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CacheDemo.Tests.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly CreateProductCommandHandler _handler;
    private readonly Mock<IProductRepository> _repositoryMock;

    public CreateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _distributedCacheMock = new Mock<IDistributedCache>();
        _handler = new CreateProductCommandHandler(
            _repositoryMock.Object,
            _distributedCacheMock.Object
        );
    }

    [Fact]
    public async Task Handle_CreatesProduct_AndInvalidatesCache()
    {
        // Arrange
        var command = new CreateProductCommand(
            "New Product",
            "Description",
            199.99m,
            25
        );

        _repositoryMock.Setup(r => r.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .Returns((Product p, CancellationToken ct) => Task.FromResult(p));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var product = result.Data;

        // Assert
        Assert.NotNull(product);
        Assert.Equal("New Product", product.Name);
        Assert.Equal("Description", product.Description);
        Assert.Equal(199.99m, product.Price);
        Assert.Equal(25.00, product.Stock);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Once);


        // Verify cache
        _distributedCacheMock.Verify(
            c => c.RemoveAsync(
                "all-products",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}