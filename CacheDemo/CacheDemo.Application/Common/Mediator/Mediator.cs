using CacheDemo.Application.Common.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CacheDemo.Application.Common.Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));

        if (handleMethod == null)
            throw new InvalidOperationException($"Handler for {requestType.Name} does not have a Handle method");

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });

        if (result is Task<TResponse> task) return await task;

        throw new InvalidOperationException(
            $"Handler for {requestType.Name} did not return a Task<{typeof(TResponse).Name}>");
    }
}