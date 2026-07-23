using Bssy.Mediatr.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bssy.Mediatr.Core.Tests
{
    public class MediatorSendTests
    {
        class PingRequest : IRequest { }

        class PingRequestHandler : IRequestHandler<PingRequest>
        {
            public bool WasCalled { get; private set; }

            public Task Handle(PingRequest request, CancellationToken cancellationToken)
            {
                WasCalled = true;
                return Task.CompletedTask;
            }
        }


        [Fact]
        public async Task Test1()
        {
            var handler = new PingRequestHandler();
            var services = new ServiceCollection();
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<IRequestHandler<PingRequest>>( _ => handler);

            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();


            await mediator.Send(new PingRequest());

            Assert.True(handler.WasCalled);

        }
    }
}
