namespace Bssy.Mediatr.Core.Abstractions
{
    public interface IBaseRequest
    {
    }
    public interface IRequest : IBaseRequest
    {
    }

    public interface IRequest<TResponse> : IBaseRequest
    {
    }
}
