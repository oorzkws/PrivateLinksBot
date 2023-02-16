namespace PrivateLinksBot;


public class ImgurUrlProvider : UrlProviderBase {
    public ImgurUrlProvider(UrlProviderService service) : base(service) {
        Name = "rimgo";
        FriendlyName = "Imgur - Rimgo";
        SecondaryUrls = new[]{"https://rimgo.pussthecat.org"};
        UrlPatterns = new[] {@"^https?:\/{2}([im]\.)?imgur\.(com|io)(\/|$)"};
    }
}