using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PrivateLinksBot; 

public static class ServiceCollectionExtensions {
    public static IServiceCollection RegisterImplicitServices<T>(this IServiceCollection collection) {
        var interfaceType = typeof(T);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!interfaceType.IsAssignableFrom(type))
                continue;
            if (type.IsAbstract)
                continue;
            
            collection.TryAddEnumerable(ServiceDescriptor.Singleton(interfaceType, type));
        }
        
        return collection;
    }
}