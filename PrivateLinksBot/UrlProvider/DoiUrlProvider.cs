using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider; 

public class DoiUrlProvider  : UrlProviderBase {

    public DoiUrlProvider(DiscordSocketClient client) : base(client) {
        ServiceNameFriendly = "DOI/ISO26324 - Sci-hub";
        ServiceName = "sci-hub";
        FallbackUrl = "https://sci-hub.ru";
        TestUrlSuffix = "/about";
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|)doi\.org(\/|$)"
        };
    }
    
    public override string GetRandomInstance(Uri url) {
        var tlds = new[] {"st", "ru"};
        var instance = $"https://sci-hub.{tlds[new Random().Next(0, 2)]}/";
        return $"{instance}{url}";
    }
}