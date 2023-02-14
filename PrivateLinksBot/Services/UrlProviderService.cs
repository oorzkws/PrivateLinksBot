using System.Reflection;
using Discord.WebSocket;
using PrivateLinksBot.UrlProvider;
using static PrivateLinksBot.UrlProvider.UrlProviderBroker;

namespace PrivateLinksBot;

public class UrlProviderService : ServiceBase {
    private IServiceProvider Provider { get; }

    public UrlProviderService(DiscordSocketClient client, IServiceProvider provider) : base(client) {
        Provider = provider;
    }

    public override async Task InitializeAsync() {
        var interfaceType = typeof(UrlProviderBase);
        UrlProviderBases = new List<UrlProviderBase>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!interfaceType.IsAssignableFrom(type))
                continue;
            if (type.IsAbstract)
                continue;

            var instance = (UrlProviderBase) Activator.CreateInstance(type, args: Client)!;

            UrlProviderBases.Add(instance);
        }

        var connectionTimer = new System.Timers.Timer(TimeSpan.FromSeconds(5));
        connectionTimer.Enabled = true;
        connectionTimer.AutoReset = true;
        connectionTimer.Elapsed += (_, _) => {
            var targetService = GetRandomService(HasServiceData);
            var targetUrl = targetService!.GetRandomInstance(true);

            try {
                var client = new HttpClient {Timeout = targetService.TestTimeoutSpan};
                var message = new HttpRequestMessage(HttpMethod.Head, targetUrl + targetService.TestUrlSuffix);
                var response = client.Send(message);
                if (response.IsSuccessStatusCode) {
                    Logger.LogDebug($"Successfully verified service: {targetUrl}");
                    UrlBlacklist.Remove(targetUrl);
                }
                else if (!UrlBlacklist.Contains(targetUrl)) {
                    Logger.LogDebug($"Service {targetUrl} failed testing");
                    UrlBlacklist.Add(targetUrl);
                }
            }
            catch (HttpRequestException e) {
                Logger.LogDebug($"Service {targetUrl} failed testing with exception {e.Message}");
                if (!UrlBlacklist.Contains(targetUrl))
                    UrlBlacklist.Add(targetUrl);
            }
        };

        await base.InitializeAsync();
    }
}