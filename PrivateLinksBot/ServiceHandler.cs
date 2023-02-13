using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace PrivateLinksBot;

/// <summary>
/// Finds and registers services
/// </summary>
public static class ServiceHandler
{
    public class ServiceInterface
    {
        private readonly IEnumerable<ServiceBase> services;
        
        public ServiceInterface(IEnumerable<ServiceBase> registeredServices)
        {
            services = registeredServices;
        }

        public async Task ActivateAsync()
        {
            foreach(var service in services)
            {
                await service.InitializeAsync();
            }
        }
    }

    public static IServiceCollection RegisterImplicitServices(this IServiceCollection collection)
    {
        var interfaceType = typeof(ServiceBase);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!interfaceType.IsAssignableFrom(type))
                continue;
            if (type.IsAbstract)
                continue;

            collection.AddSingleton(interfaceType, type);
        }
        collection.AddSingleton(typeof(ServiceInterface));
        return collection;
    }
}