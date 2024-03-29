﻿using System.Net;
using System.Reflection;
using Discord.WebSocket;
using Timer = System.Timers.Timer;
using static System.TimeSpan;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

public class UrlProviderService : ServiceBase {
    public readonly Dictionary<string, UrlProviderBase> Providers = new();
    public readonly List<string> Blacklist = new();

    private const string dataPath = @"https://raw.githubusercontent.com/libredirect/instances/main/data.json";

    public UrlProviderService(DiscordSocketClient client, IServiceProvider parent) : base(client) {
    }

    public override async Task InitializeAsync() {
        var interfaceType = typeof(UrlProviderBase);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!interfaceType.IsAssignableFrom(type)) {
                continue;
            }

            if (type.IsAbstract) {
                continue;
            }

            var instance = (UrlProviderBase) Activator.CreateInstance(type, this)!;

            Providers[instance.Name] = instance;
        }

        // We can load our config now that we know our services
        // TODO: See if the reflection and config load can be parallelized
        _ = ReloadConfig();

        // We'll check a random instance every 5 seconds
        var instanceCheckTimer = new Timer(FromSeconds(5)) {
            Enabled = true,
            AutoReset = true
        };

        instanceCheckTimer.Elapsed += async (_, _) => {
            // Filter to providers with primary (dynamic, loaded from config) instances
            var servicesWithDynamicInstances = (
                from provider in Providers.Values
                where provider.PrimaryUrls is not null
                where provider.PrimaryUrls!.Length > 0
                select provider).ToArray();

            // ??? how did you do this
            if (servicesWithDynamicInstances.Length == 0) {
                return;
            }

            // We already null-tested above
            var service = servicesWithDynamicInstances.RandomEntry();
            var instance = service.PrimaryUrls!.RandomEntry();

            // Poll the server with a HEAD request and see what happens
            try {
                var client = new HttpClient {
                    Timeout = FromSeconds(service.ConnectionTimeoutSeconds)
                };
                var message = new HttpRequestMessage(HttpMethod.Head, instance + service.TestEndpoint);
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode ||
                    (service.Name == "libreddit" && response.StatusCode == HttpStatusCode.NotFound)) {
                    LogDebug("Instance Check", $"Successfully tested {service.Name} instance {instance}");
                    Blacklist.Remove(instance);
                }
                else if (!Blacklist.Contains(instance)) {
                    throw new HttpRequestException($"{response.StatusCode}");
                }
            }
            catch (Exception e) {
                LogDebug("Instance Check", $"Unsuccessfully tested {service.Name} instance {instance} ({e.Message})");
                if (!Blacklist.Contains(instance)) {
                    Blacklist.Add(instance);
                }
            }
        };


        // Reload our web-based config every 4 hours in case we manage to go a while without crashing :^)
        var configReloadTimer = new Timer(FromMinutes(240)) {
            Enabled = true,
            AutoReset = true
        };

        // Since the signature isn't compatible we just wrap in a lambda
        configReloadTimer.Elapsed += async (_, _) => await ReloadConfig();

        await base.InitializeAsync();
    }

    /// <summary>
    /// Fetches the service configuration data from the internet, falling back to on-disk if necessary
    /// </summary>
    private async Task ReloadConfig() {
        LogInfo("UrlProvider", "Reloading configuration");
        // Try the URL method first
        var parsedData = await Util.ReadJsonFile<Dictionary<string, Dictionary<string, string[]>>>(dataPath);
        // Welp, try for a file in the working directory
        parsedData ??=
            await Util.ReadJsonFile<Dictionary<string, Dictionary<string, string[]>>>(Path.GetFileName(dataPath));
        if (parsedData is null) {
            LogWarning("UrlProvider",
                $"Both remote and local attempts at reading data.json have failed. Please fetch `{dataPath}` and place it in the working directory.");
            return;
        }

        // Huzzah, add the data to our relevant classes
        foreach (var service in Providers.Values) {
            // Check if our service is actually present
            if (!parsedData.TryGetValue(service.Name, out var instanceList)) {
                continue;
            }

            instanceList.TryGetValue("clearnet", out service.PrimaryUrls);
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public UrlProviderBase? GetRandomApplicableService(string urlString, Uri? url) {
        // Not handed a url, check if the string is valid
        if (url is null && !Uri.TryCreate(urlString, UriKind.Absolute, out url)) {
            return null;
        }

        // Do we have any applicable providers?
        var validProviders = Providers.Values.Where(p => p.IsApplicable(urlString)).ToArray();
        return validProviders.Length == 0 ? null : validProviders.RandomEntry();
    }


    public string? GetLinkFromRandomApplicableService(string urlString) =>
        // Not a url?
        !Uri.TryCreate(urlString, UriKind.Absolute, out var url)
            ? null
            : GetRandomApplicableService(urlString, url)?.GetLinkFromRandomInstance(url);
}