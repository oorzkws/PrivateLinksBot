using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public class ImgurUrlProvider : UrlProviderBase
{
    public ImgurUrlProvider(DiscordSocketClient client) : base(client)
    {
        ServiceNameFriendly = "Imgur - Rimgo";
        ServiceName = "rimgo";
        FallbackUrl = "https://rimgo.pussthecat.org";
        UrlPatterns = new[] {@"^https?:\/{2}([im]\.)?imgur\.(com|io)(\/|$)"};
    }
}