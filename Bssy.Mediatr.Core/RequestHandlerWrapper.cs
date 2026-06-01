using Bssy.Mediatr.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bssy.Mediatr.Core
{
    public class RequestHandlerWrapper<TRequest>: ARequestHandlerWrapper where TRequest : IRequest
    {
        public override async Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

            Func<Task> next = () => handler.Handle((TRequest)request, cancellationToken);

            var behaviors = serviceProvider.GetServices<IBssyMPipelineBehavior<TRequest>>();

            foreach (var behavior in behaviors.Reverse())
            {
                var current = next;
                next = () => behavior.Handle((TRequest)request, current, cancellationToken);
            }

            await next();
        }
    }
    public class RequestHandlerWrapper<TRequest, TResponse> : ARequestHandlerWrapper<TResponse> where TRequest : IRequest<TResponse>
    {
        public override async Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            Func<Task<TResponse>> next = () => handler.Handle((TRequest)request, cancellationToken);

            var behaviors = serviceProvider.GetServices<IBssyMPipelineBehavior<TRequest, TResponse>>();

            foreach (var behavior in behaviors.Reverse())
            {
                var current = next;
                next = () => behavior.Handle((TRequest)request, current, cancellationToken);
            }

            return await next();
        }
    }
}
