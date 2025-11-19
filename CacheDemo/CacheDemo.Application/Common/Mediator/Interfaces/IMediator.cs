namespace CacheDemo.Application.Common.Mediator.Interfaces;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}