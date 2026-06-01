
using Bssy.Mediatr.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bssy.Mediatr.Core
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddBssyMediatrCore(params Assembly[] assemblies)
            {
                if (assemblies == null || assemblies.Length == 0)
                {
                    assemblies = Assembly.GetCallingAssembly().GetReferencedAssemblies()
                        .Select(Assembly.Load)
                        .ToArray();
                }

                services.AddScoped<IMediator, Mediator>();

                var handlerInterfaces = new[]
                {
                    typeof(IRequestHandler<,>),
                    typeof(IRequestHandler<>),
                    typeof(INotificationHandler<>)
                };

                foreach(var handlerInterface in handlerInterfaces)
                {
                    var types = assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsClass && !t.IsAbstract)
                        .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface));
                    foreach (var type in types)
                    {
                        var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);
                        foreach (var @interface in interfaces)
                        {
                            services.AddScoped(@interface, type);
                        }
                    }
                }

                return services;

            }
        }
    }
}
