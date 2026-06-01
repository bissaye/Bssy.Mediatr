using Bssy.Mediatr.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Bssy.Mediatr.Core
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, object> _requestCache = new();
        private static readonly ConcurrentDictionary<Type, object> _requestResponseCache = new();
        private static readonly ConcurrentDictionary<Type, object> _notificationCache = new();
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var notificationType = notification.GetType();
            var wrapper = _notificationCache.GetOrAdd(
                notificationType,
                type => Activator.CreateInstance(
                    typeof(NotificationHandlerWrapper<>).MakeGenericType(notificationType)
                )!);
            await ((ANotificationHandlerWrapper)wrapper).Handle(notification, _serviceProvider, cancellationToken);
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            var requestType = request.GetType();
            var wrapper = _requestCache.GetOrAdd(
                requestType,
                type => Activator.CreateInstance(
                    typeof(RequestHandlerWrapper<>).MakeGenericType(requestType)
                )!);
            await ((ARequestHandlerWrapper)wrapper).Handle(request, _serviceProvider, cancellationToken);
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();

            var wrapper = _requestResponseCache.GetOrAdd(
                requestType,
                type => Activator.CreateInstance(
                    typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse))
                )!);

            return await ((ARequestHandlerWrapper<TResponse>)wrapper)
                .Handle(request, _serviceProvider, cancellationToken);
        }

    }
}
