// using Timer = System.Timers.Timer;
//
// namespace PrivateLinksBot.UrlProvider;
//
// public static class UrlProviderBroker {
//     private const string dataUrl = 
//     public static List<UrlProviderBase> UrlProviderBases = new();
//     public static List<string> UrlBlacklist = new();
//     public static Dictionary<string, string[]> ServiceData = new();
//
//     static UrlProviderBroker() {
//         // Load config and reload approx every 4h
//         ReloadConfig();
//
//         configTimer.Elapsed += (_, _) => ReloadConfig();
//     }
//
//
//     
//     public static bool HasServiceData(UrlProviderBase service) => ServiceData.ContainsKey(service.ServiceName);
//
//     public static UrlProviderBase? GetRandomService(Func<UrlProviderBase, bool> predicate) {
//         var serviceList = UrlProviderBases.Where(predicate).ToList();
//         var serviceCount = serviceList.Count;
//         if (serviceCount == 0) {
//             return null;
//         }
//
//         var randomIndex = new Random().Next(0, serviceList.Count - 1);
//         return serviceList[randomIndex];
//     }
//
//     public static UrlProviderBase? GetRandomServiceForUrl(Uri url) {
//         return GetRandomService(s => s.IsApplicable(url.ToString()));
//     }
//
//     public static string? GetNewUrlFromRandomService(string urlString) {
//         // Not a url
//         if (!Uri.TryCreate(urlString, UriKind.Absolute, out var url)) return null;
//
//         var randomService = GetRandomServiceForUrl(url);
//
//         return randomService?.GetRandomInstance(url);
//     }
// }