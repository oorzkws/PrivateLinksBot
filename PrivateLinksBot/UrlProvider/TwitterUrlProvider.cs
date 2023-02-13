using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public class TwitterUrlProvider : UrlProviderBase
{
    public TwitterUrlProvider(DiscordSocketClient client) : base(client)
    {
        ServiceNameFriendly = "Twitter - Nitter";
        ServiceName = "nitter";
        FallbackUrl = "https://nitter.nl";
        TestUrlSuffix = "/about";
        UrlPatterns = new[]
        {
            @"^https?:\/{2}(www\.|mobile\.|)twitter\.com(\/|$)",
            @"^https?:\/{2}(pbs\.|video\.|)twimg\.com(\/|$)",
            @"^https?:\/{2}platform\.twitter\.com/embed(\/|$)",
            @"^https?:\/{2}t\.co(\/|$)"
        };
    }
}