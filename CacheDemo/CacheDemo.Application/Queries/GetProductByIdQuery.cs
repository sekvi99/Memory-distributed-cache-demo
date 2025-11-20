using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;
using CacheDemo.Application.DTOs;

namespace CacheDemo.Application.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;