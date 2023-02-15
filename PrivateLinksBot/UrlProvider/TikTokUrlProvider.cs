namespace PrivateLinksBot.UrlProvider;

public class TikTokUrlProvider : UrlProviderBase {
    public TikTokUrlProvider(UrlProviderService service) : base(service) {
        Name = "proxiTok";
        FriendlyName = "TikTok - ProxiTok";
        SecondaryUrls = new[]{"https://tt.vern.cc"};
        UrlPatterns = new[] {
            @"^https?:\/{2}(www\.|)tiktok\.com(\/|$)"
        };
    }
}