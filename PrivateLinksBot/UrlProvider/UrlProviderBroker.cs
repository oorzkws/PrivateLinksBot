using Timer = System.Timers.Timer;

namespace PrivateLinksBot.UrlProvider;

public static class UrlProviderBroker {
    private const string dataUrl = @"https://raw.githubusercontent.com/libredirect/instances/main/data.json";
    public static List<UrlProviderBase> UrlProviderBases = new();
    public static List<string> UrlBlacklist = new();
    public static Dictionary<string, string[]> ServiceData = new();

    static UrlProviderBroker() {
        // Load config and reload approx every 4h
        ReloadConfig();
        var configTimer = new Timer(TimeSpan.FromMinutes(240)) {
            Enabled = true,
            AutoReset = true
        };
        configTimer.Elapsed += (_, _) => ReloadConfig();
    }

    private static void ReloadConfig() {
        Logger.LogInfo("Reloading UrlProviderBroker configuration");
        var parsedData = Util.ReadJsonFile<Dictionary<string, Dictionary<string, string[]>>>(dataUrl);
        // Fallback to local
        if (parsedData is null) {
            // Split the path by / and grab the last segment
            var filePath = dataUrl.Split("/").Last();
            parsedData = Util.ReadJsonFile<Dictionary<string, Dictionary<string, string[]>>>(filePath);
        }
        if (parsedData is not null) {
            foreach (var service in parsedData) {
                if (!service.Value.ContainsKey("clearnet"))
                    continue;
                ServiceData.Add(service.Key, service.Value["clearnet"]);
            }
        }
        else {
            Logger.LogWarning($"Both remote and local attempts at reading data.json have failed. Please fetch `{dataUrl}` and place it in the working directory.");
        }
    }
    
    public static bool HasServiceData(UrlProviderBase service) => ServiceData.ContainsKey(service.ServiceName);

    public static UrlProviderBase? GetRandomService(Func<UrlProviderBase, bool> predicate) {
        var serviceList = UrlProviderBases.Where(predicate).ToList();
        var serviceCount = serviceList.Count;
        if (serviceCount == 0) {
            return null;
        }

        var randomIndex = new Random().Next(0, serviceList.Count - 1);
        return serviceList[randomIndex];
    }

    public static UrlProviderBase? GetRandomServiceForUrl(Uri url) {
        return GetRandomService(s => s.IsApplicable(url.ToString()));
    }

    public static string? GetNewUrlFromRandomService(string urlString) {
        // Not a url
        if (!Uri.IsWellFormedUriString(urlString, UriKind.RelativeOrAbsolute)) return null;

        var url = new Uri(urlString);

        var randomService = GetRandomServiceForUrl(url);

        return randomService?.GetRandomInstance(url);
    }
}