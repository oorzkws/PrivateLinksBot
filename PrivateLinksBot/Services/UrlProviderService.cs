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
            var services = new List<KeyValuePair<string, string>>();

            foreach (var set in UrlProviderBases) {
                foreach (var service in ServiceData[set.ServiceName]) {
                    services.Add(new KeyValuePair<string, string>(service, set.TestUrlSuffix));
                }
            }

            var index = new Random().Next(0, services.Count - 1);
            var randomService = services[index];
            try {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response =
                    client.Send(new HttpRequestMessage(HttpMethod.Head, randomService.Key + randomService.Value));
                if (response.IsSuccessStatusCode) {
                    Logger.LogDebug($"Successfully verified service: {randomService.Key}");
                    UrlBlacklist.Remove(randomService.Key);
                }
                else if (!UrlBlacklist.Contains(randomService.Key)) {
                    Logger.LogDebug($"Service {randomService.Key} failed testing");
                    UrlBlacklist.Add(randomService.Key);
                }
            }
            catch (HttpRequestException e) {
                Logger.LogDebug($"Service {randomService.Key} failed testing with exception {e.Message}");
                if (!UrlBlacklist.Contains(randomService.Key))
                    UrlBlacklist.Add(randomService.Key);
            }
        };

        await base.InitializeAsync();
    }
}