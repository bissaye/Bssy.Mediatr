namespace Bssy.Mediatr.Core.Abstractions
{
    public abstract class ANotificationHandlerWrapper
    {
        public abstract Task Handle(INotification notification,
                            IServiceProvider serviceProvider,
                            CancellationToken cancellationToken);
    }
}
