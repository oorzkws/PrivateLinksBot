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

    public static string[] TryGetService(string service, string defaultService) {
        if (ServiceData.TryGetValue(service, out string[]? ret)) {
            return ret;
        }

        return new[] {defaultService};
    }

    public static bool HasApplicableService(string url) {
        return UrlProviderBases.Any(p => p.IsApplicable(url));
    }

    public static string? RequestUrl(string url) {
        var validServices = (from provider in UrlProviderBases
            where provider.IsApplicable(url)
            select provider).ToList();

        if (validServices.Count == 0)
            return null;

        var index = new Random().Next(0, validServices.Count - 1);
        return validServices[index].RequestUrl(url);
    }
}