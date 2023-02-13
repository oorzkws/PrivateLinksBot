using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public class LibRedditUrlProvider : UrlProviderBase {
    public LibRedditUrlProvider(DiscordSocketClient client) : base(client) {
        ServiceNameFriendly = "Reddit - Libreddit";
        ServiceName = "libreddit";
        FallbackUrl = "https://libreddit.spike.codes";
        TestUrlSuffix = "/settings";
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|old\.|np\.|new\.|amp\.|)(reddit|reddittorjg6rue252oqsxryoxengawnmo46qy4kyii5wtqnwfj4ooad)\.(com|onion)(?=\/u(ser)?\/|\/r\/|\/search|\/new|\/?$)",
            @"^https?:\/{2}(i|(external-)?preview)\.redd\.it"
        };
    }

    public override string RequestUrl(Uri url) {
        return url.Host.Split('.')[0] switch {
            "preview" => $"{RandomInstance()}/preview/pre{url.PathAndQuery}",
            "external-preview" => $"{RandomInstance()}/preview/external-pre{url.PathAndQuery}",
            "i" => $"{RandomInstance()}/img{url.AbsolutePath}",
            _ => RandomInstance() + url.PathAndQuery,
        };
    }
}