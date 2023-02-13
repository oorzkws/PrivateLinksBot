using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public class InvidiousUrlProvider : BaseYoutubeProvider
{
    public InvidiousUrlProvider(DiscordSocketClient client) : base(client)
    {
        ServiceNameFriendly = "YouTube - Invidious";
        ServiceName = "invidious";
        TestUrlSuffix = "/privacy";
        FallbackUrl = "https://yewtu.be";
    }
    
}