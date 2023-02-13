using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public abstract class UrlProviderBase {
    public string ServiceNameFriendly { get; protected set; } = string.Empty;
    public string ServiceName { get; protected set; } = string.Empty;
    public string FallbackUrl { get; protected set; } = string.Empty;
    public string TestUrlSuffix { get; protected set; } = string.Empty;
    public string[] UrlPatterns { get; protected set; } = { };

    protected UrlProviderBase(DiscordSocketClient client) {
    }

    public virtual bool IsApplicable(string urlString) =>
        UrlPatterns.Any(str => Regex.Match(urlString, str).Success);

    public virtual string RandomInstance() {
        if (!UrlProviderBroker.ServiceData.TryGetValue(ServiceName, out var serviceUrls))
            serviceUrls = new[] {FallbackUrl};

        var cleanUrls = serviceUrls.Except(UrlProviderBroker.UrlBlacklist).ToList();
        if (cleanUrls.Count == 0)
            throw new IndexOutOfRangeException($"UrlProviderBase {GetType().Name} has no non-blacklisted ServiceUrls");

        var index = new Random().Next(0, cleanUrls.Count - 1);

        return cleanUrls[index];
    }

    public virtual string RequestUrl(Uri url) {
        return RandomInstance() + url.PathAndQuery;
    }

    public virtual string RequestUrl(string url) => RequestUrl(new Uri(url));
}