using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot; 

public static class ServiceCollectionExtensions {
    public static IServiceCollection RegisterImplicitServices<T>(this IServiceCollection collection) {
        var interfaceType = typeof(T);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (type.IsAbstract)
                continue;
            if (!type.IsSubclassOf(interfaceType))
                continue;
            collection.TryAddEnumerable(ServiceDescriptor.Singleton(interfaceType, type));
        }
        foreach(var service in collection) {
            LogDebug("ServiceCollection", $"Service: {service.ImplementationType} {service.ServiceType} as {service.Lifetime}");
        }
        return collection;
    }
}