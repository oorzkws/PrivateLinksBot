using Microsoft.Extensions.DependencyInjection;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

public static class ServiceProviderExtensions {
    public static async Task ActivateAsync<T>(this IServiceProvider serviceProvider)
        where T : ServiceBase {
        foreach (var service in serviceProvider.GetServices<T>()) {
            LogDebug("ServiceProvider", $"Activating {service.GetType()}");
            await service.InitializeAsync();
        }
    }

    public static TService GetRequiredConcreteService<TService, TServiceBase>(this IServiceProvider serviceProvider)
        where TService : TServiceBase => (from service in serviceProvider.GetServices<TServiceBase>()
        where service is not null
        where service is TService
        select (TService) service).First();
}