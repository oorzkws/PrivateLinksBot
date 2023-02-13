using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public class TikTokUrlProvider : UrlProviderBase
{
    public TikTokUrlProvider(DiscordSocketClient client) : base(client)
    {
        ServiceNameFriendly = "TikTok - ProxiTok";
        ServiceName = "proxiTok";
        FallbackUrl = "https://tt.vern.cc";
        UrlPatterns = new[]
        {
            @"^https?:\/{2}(www\.|)tiktok\.com(\/|$)"
        };
    }
}