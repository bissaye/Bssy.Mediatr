
namespace Bssy.Mediatr.Core.Abstractions
{
    public interface IBssyMPipelineBehavior<TRequest> where TRequest : IRequest
    {
        Task Handle(TRequest request, Func<Task> next, CancellationToken cancellationToken);
    }

    public interface IBssyMPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
    }
}
