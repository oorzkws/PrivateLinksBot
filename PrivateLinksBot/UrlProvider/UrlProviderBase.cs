using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace PrivateLinksBot.UrlProvider;

public abstract class UrlProviderBase {
    public string ServiceNameFriendly { get; protected set; } = string.Empty;
    public string ServiceName { get; protected set; } = string.Empty;
    public string FallbackUrl { get; protected set; } = string.Empty;
    public string TestUrlSuffix { get; protected set; } = string.Empty;
    public TimeSpan TestTimeoutSpan { get; protected set; } = TimeSpan.FromSeconds(2);
    public string[] UrlPatterns { get; protected set; } = { };

    protected UrlProviderBase(DiscordSocketClient client) {
    }

    public virtual bool IsApplicable(string urlString) =>
        UrlPatterns.Any(str => Regex.Match(urlString, str).Success);

    public virtual string GetRandomInstance(bool ignoreBlacklist = false) {
        if (!UrlProviderBroker.ServiceData.TryGetValue(ServiceName, out var serviceUrls))
            serviceUrls = new[] {FallbackUrl};

        var cleanUrls = ignoreBlacklist ? serviceUrls : serviceUrls.Except(UrlProviderBroker.UrlBlacklist).ToArray();
        if (cleanUrls.Length == 0)
            throw new IndexOutOfRangeException($"UrlProviderBase {GetType().Name} has no non-blacklisted ServiceUrls");

        var index = new Random().Next(0, cleanUrls.Length - 1);

        return cleanUrls[index] + "";
    }

    public virtual string GetRandomInstance(Uri url) => GetRandomInstance() + url.PathAndQuery;
}