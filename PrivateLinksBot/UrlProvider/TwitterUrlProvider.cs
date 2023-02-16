namespace PrivateLinksBot;


public class TwitterUrlProvider : UrlProviderBase {
    public TwitterUrlProvider(UrlProviderService service) : base(service) {
        Name = "nitter";
        FriendlyName = "Twitter - Nitter";
        SecondaryUrls = new[]{"https://nitter.nl"};
        TestEndpoint = "/about";
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|mobile\.|)twitter\.com(\/|$)",
            @"^https?:\/{2}(pbs\.|video\.|)twimg\.com(\/|$)",
            @"^https?:\/{2}platform\.twitter\.com/embed(\/|$)",
            @"^https?:\/{2}t\.co(\/|$)"
        };
    }
}