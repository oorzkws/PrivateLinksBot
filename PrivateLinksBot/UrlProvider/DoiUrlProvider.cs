namespace PrivateLinksBot.UrlProvider; 

public class DoiUrlProvider  : UrlProviderBase {

    public DoiUrlProvider(UrlProviderService service) : base(service) {
        Name = "sci-hub";
        FriendlyName = "DOI/ISO26324 - Sci-hub";
        SecondaryUrls = new[]{"https://sci-hub.ru", "https://sci-hub.st"};
        TestEndpoint = "/about";
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|)doi\.org(\/|$)"
        };
    }

    protected override string GetLink(string instance, Uri url) {
        return $"{instance}{url}";
    }
}