
namespace Bssy.Mediatr.Core.Abstractions
{
    public abstract class ARequestHandlerWrapper    
    {
        public abstract Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
    public abstract class ARequestHandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken); 
    }
}
