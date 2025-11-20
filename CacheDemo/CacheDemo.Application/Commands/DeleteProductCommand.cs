using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.Common.Models;

namespace CacheDemo.Application.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<Result<bool>>;