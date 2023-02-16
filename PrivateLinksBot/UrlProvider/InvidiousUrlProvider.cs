namespace PrivateLinksBot;

public class InvidiousUrlProvider : BaseYoutubeProvider {
    public InvidiousUrlProvider(UrlProviderService service) : base(service) {
        Name = "invidious";
        FriendlyName = "YouTube - Invidious";
        TestEndpoint = "/privacy";
        SecondaryUrls = new[] {"https://yewtu.be"};
    }
}