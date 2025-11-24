# Memory-distributed-cache-demo

This repository is a small demo showing how to combine an in-memory and a distributed cache (e.g. Redis) in a .NET application.

It contains a minimal product catalog example implemented with a clean architecture split into Application, Domain, Infrastructure, WebAPI and Tests projects.

**What this demo shows**
- **Cache layering**: query handlers read from distributed cache first, fall back to repository, and populate cache.
- **Cache invalidation**: command handlers invalidate related cache entries when data changes.
- **Unit testing**: tests show how to mock `IProductRepository` and `IDistributedCache` to verify behavior.

**Repository layout**
- `CacheDemo/CacheDemo.sln` : solution file.
- `CacheDemo/CacheDemo.Application` : application layer (commands, queries, handlers, DTOs).
- `CacheDemo/CacheDemo.Domain` : domain layer (entities, repository interfaces).
- `CacheDemo/CacheDemo.Infrastructure` : implementations (EF Core DbContext, repository implementations, DI).
- `CacheDemo/CacheDemo.WebAPI` : ASP.NET Core Web API exposing product endpoints.
- `CacheDemo/CacheDemo.Tests` : xUnit test project demonstrating handler tests.

Key files to look at:
- `CacheDemo.Application/Queries/Handlers/GetAllProductsQueryHandler.cs` : reads `all-products` cache key and writes it on miss.
- `CacheDemo.Application/Queries/Handlers/GetProductByIdQueryHandler.cs` : reads `product-{id}` key and returns cached DTO if present.
- `CacheDemo.Application/Commands/Handlers/*` : command handlers invalidate `all-products` and `product-{id}` where appropriate.

**Prerequisites**
- .NET SDK (7.0+ recommended). Verify with `dotnet --version`.
- (Optional) Docker + Docker Compose if you want to run a Redis container.

**Run locally (Web API)**
1. From repository root, restore/build/run using PowerShell:

```powershell
dotnet restore "CacheDemo/CacheDemo.sln"
dotnet build "CacheDemo/CacheDemo.sln"
dotnet run --project "CacheDemo/CacheDemo.WebAPI" --configuration Debug
```

2. By default the Web API uses the configured cache provider in `CacheDemo.Infrastructure` — to use Redis you can start a Redis container with `docker-compose up` (there's a `docker-compose.yml` at the repository root).

```powershell
docker-compose up -d
```

**Run tests**
Run the unit tests with:

```powershell
dotnet test "CacheDemo/CacheDemo.sln"
```

**Tests added / maintained**
- Handler tests mock `IProductRepository` and `IDistributedCache` (see `CacheDemo.Tests/Queries` and `CacheDemo.Tests/Commands`).

**Notes & tips**
- Cache keys used in the demo: `all-products` and `product-{id}`.
- When changing handlers, update tests to assert both repository interactions and cache invalidation behavior.
- The tests use `Moq` and `xUnit`.

If you'd like, I can:
- Run the test suite and report results.
- Add a short Postman collection snippet for the API endpoints.
- Wire a simple local Redis docker-compose service and show sample requests.

Enjoy exploring the demo — ask if you want me to run tests or add a Redis setup for you.