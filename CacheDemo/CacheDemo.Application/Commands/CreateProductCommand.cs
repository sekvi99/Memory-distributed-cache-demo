using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;

namespace CacheDemo.Application.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock
) : IRequest<Result<ProductDto>>;