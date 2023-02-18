// ReSharper disable StringLiteralTypo

namespace PrivateLinksBot;

public class LibRedditUrlProvider : UrlProviderBase {
    public LibRedditUrlProvider(UrlProviderService service) : base(service) {
        // Set metadata
        Name = "libreddit";
        FriendlyName = "Reddit - Libreddit";
        SecondaryUrls = new[] {"https://libreddit.spike.codes"};
        TestEndpoint = "/settings";
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|old\.|np\.|new\.|amp\.|)(reddit|reddittorjg6rue252oqsxryoxengawnmo46qy4kyii5wtqnwfj4ooad)\.(com|onion)(?=\/u(ser)?\/|\/r\/|\/search|\/new|\/?$)",
            @"^https?:\/{2}(i|(external-)?preview)\.redd\.it"
        };
        ConnectionTimeoutSeconds = 10;
    }

    protected override string GetLink(string instance, Uri url) {
        return url.Host.Split('.')[0] switch {
            "preview" => $"{instance}/preview/pre{url.PathAndQuery}",
            "external-preview" => $"{instance}/preview/external-pre{url.PathAndQuery}",
            "i" => $"{instance}/img{url.AbsolutePath}",
            _ => base.GetLink(instance, url)
        };
    }
}