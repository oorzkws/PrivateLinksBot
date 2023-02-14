using System.Text;
using System.Text.Json;

namespace PrivateLinksBot.UrlProvider;

public static class UrlProviderBroker {
    public static List<UrlProviderBase> UrlProviderBases = new();
    public static List<string> UrlBlacklist = new();
    public static Dictionary<string, string[]> ServiceData = new();

    static UrlProviderBroker() {
        if (File.Exists("./data.json")) {
            var text = File.ReadAllText("./data.json", Encoding.UTF8);
            var parsedData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(text);
            if (parsedData is not null) {
                foreach (var service in parsedData) {
                    if (!service.Value.ContainsKey("clearnet"))
                        continue;
                    ServiceData.Add(service.Key, service.Value["clearnet"]);
                }
            }
        }
        else {
            Logger.LogInfo("No data.json found in working directory.");
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
        // No service handles given url
        if (randomService is null)
            return null;

        return randomService.GetRandomInstance(url);
    }
}