using System.Text.RegularExpressions;

namespace PrivateLinksBot;

public abstract class UrlProviderBase {
    private readonly UrlProviderService service;
    public string Name;
    public string FriendlyName;
    public string[]? PrimaryUrls;
    public string[] SecondaryUrls;
    public string[] UrlPatterns;
    public string TestEndpoint;
    public int ConnectionTimeoutSeconds = 2;
    
    // ReSharper disable once PublicConstructorInAbstractClass - required for DI
    public UrlProviderBase(UrlProviderService service) {
        Logger.LogDebug($"Constructing {GetType()}");
        this.service = service;
        // These will be overwritten in any child classes
        Name = string.Empty;
        FriendlyName = string.Empty;
        SecondaryUrls = Array.Empty<string>();
        UrlPatterns = Array.Empty<string>();
        TestEndpoint = string.Empty;
    }


    public bool IsApplicable(string urlString) =>
        UrlPatterns.Any(str => Regex.Match(urlString, str).Success);

    private string GetRandomInstance(bool ignoreBlacklist = false) {
        string[]? validInstances = PrimaryUrls;
        
        // We have to do some work to build the list in this case
        if (!ignoreBlacklist && validInstances is not null) {
            // Subtract our blacklist and return the remainder to our local if it has any entries
            var cleanInstances = validInstances.Except(service.Blacklist).ToArray();
            if (cleanInstances.Length > 0) {
                validInstances = cleanInstances;
            }
        }
        
        // We have no primary list or failed to find any clean entries
        validInstances ??= SecondaryUrls;
        
        // Huzzah
        return validInstances.RandomEntry();
    }

    protected virtual string GetLink(string instance, Uri url) {
        return instance + url.PathAndQuery;
    }

    public string GetLinkFromRandomInstance(Uri url) => GetLink(GetRandomInstance(), url);
}