using Bssy.Mediatr.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bssy.Mediatr.Core
{
    public class NotificationHandlerWrapper<TNotification> : ANotificationHandlerWrapper where TNotification : INotification
    {
        public override async Task Handle(INotification notification, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var handler in handlers)
            {
                await handler.Handle((TNotification)notification, cancellationToken);
            }
        }
    }
}
