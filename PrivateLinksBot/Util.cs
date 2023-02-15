using System.Text;
using System.Text.Json;

namespace PrivateLinksBot;

public static class Util {
    private static Random random = new Random();
    
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

    public static T RandomEntry<T>(this T[] array) {
        var length = array.Length - 1;
        if (length < 0) {
            throw new ArgumentException("Array provided is empty, no valid index to return");
        }
        return array[random.Next(0, length)];
    }

    public static KeyValuePair<T1, T2> RandomEntry<T1, T2>(this Dictionary<T1, T2> dictionary) where T1 : notnull {
        var length = dictionary.Count - 1;
        if (length < 0) {
            throw new ArgumentException("Dictionary provided is empty, no valid index to return");
        }

        return dictionary.ElementAt(random.Next(0, length));
    }
}