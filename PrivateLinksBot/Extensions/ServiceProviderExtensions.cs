using Microsoft.Extensions.DependencyInjection;

namespace PrivateLinksBot; 

public static class ServiceProviderExtensions {
    public static async Task ActivateAsync<T>(this IServiceProvider serviceProvider)
        where T : ServiceBase {
        foreach (var service in serviceProvider.GetServices<T>()) {
            await service.InitializeAsync();
        }
    }
}