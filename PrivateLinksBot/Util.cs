using System.Net;
using System.Text;
using System.Text.Json;

namespace PrivateLinksBot;

public static class Util {
    public static T? ReadJsonFile<T>(string path) {
        if (!Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute)) {
            return default;
        }

        var pathAsUri = new Uri(path, UriKind.RelativeOrAbsolute);
        var isFile = pathAsUri.IsUnc || pathAsUri.Scheme == string.Empty;

        string? fileContents = null;
        if (isFile) {
            if (!File.Exists(path)) {
                return default;
            }

            fileContents = File.ReadAllText(path, Encoding.UTF8);
        }
        else {
            var webClient = new HttpClient();
            try {
                fileContents = webClient.GetStringAsync(pathAsUri).Result;
            }
            catch {
                // ignored
            }
        }

        if (fileContents is null) {
            return default;
        }

        T? parsedContents = default;
        try {
            parsedContents = JsonSerializer.Deserialize<T>(fileContents);
        }
        catch {
            // Ignored
        }

        return parsedContents;
    }
}