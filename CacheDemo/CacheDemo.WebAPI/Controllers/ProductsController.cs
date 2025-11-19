using Microsoft.AspNetCore.Mvc;

namespace CacheDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// 
    /// Get product by ID - Demonstrates IMemoryCache usage
    /// 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// 
    /// Get all products - Demonstrates IDistributedCache (Redis) usage
    /// 
    [HttpGet]
    public async Task<ActionResult<List>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllProductsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// 
    /// Create product - Demonstrates cache invalidation
    /// 
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
        
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}