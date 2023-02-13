using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public abstract class BaseYoutubeProvider : UrlProviderBase
{
    public BaseYoutubeProvider(DiscordSocketClient client) : base(client)
    {
        UrlPatterns = new[]
        {
            @"^https?:\/{2}redirect\.invidious\.io\/.*",
            @"^https?:\/{2}(?:www\.|m\.|)youtube.com(\/|$)(?!iframe_api\/|redirect\/)",
            @"^https?:\/{2}img\.youtube.com\/vi\/.*\/..*",
            @"^https?:\/{2}(?:i|s)\.ytimg.com\/vi\/.*\/..*",
            @"^https?:\/{2}(?:www\.|)youtube.com\/watch?v=..*",
            @"^https?:\/{2}youtu\.be\/..*",
            @"^https?:\/{2}(?:www\.|)(youtube|youtube-nocookie)\.com\/embed\/..*"
        };
    }
}