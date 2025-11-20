using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;

namespace CacheDemo.Application.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock
) : IRequest<Result<ProductDto>>;