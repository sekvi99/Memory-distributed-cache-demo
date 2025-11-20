using CacheDemo.Application.Commands;
using CacheDemo.Application.Common.Mediator.Interfaces;
using CacheDemo.Application.DTOs;
using CacheDemo.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CacheDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// Get product by ID - Demonstrates IMemoryCache usage
    /// First call: DB query + cache
    /// Subsequent calls: Served from memory (5 min absolute, 2 min sliding expiration)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        Response.Headers.Append("X-Cache-Source", "Memory");
        return Ok(result);
    }

    /// Get all products - Demonstrates IDistributedCache (Redis) usage
    /// First call: DB query + Redis cache
    /// Subsequent calls: Served from Redis (10 min absolute, 5 min sliding expiration)
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllProductsQuery();
        var result = await mediator.Send(query, cancellationToken);

        Response.Headers.Append("X-Cache-Source", "Redis");
        return Ok(result);
    }

    /// Create product - Demonstrates cache invalidation
    /// Invalidates: "all-products" in both memory and Redis
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Stock
        );

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    /// Update product - Demonstrates cache invalidation
    /// Invalidates: "product-{id}" and "all-products" in both caches
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Stock
        );

        var result = await mediator.Send(command, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// Delete product - Demonstrates cache invalidation
    /// Invalidates: "product-{id}" and "all-products" in both caches
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess) return NotFound(new { errors = result.Errors });

        return NoContent();
    }
}

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);