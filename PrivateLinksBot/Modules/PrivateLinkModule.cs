﻿using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class PrivateLinkModule : InteractionModuleBase {
    private readonly string[] splitTokens = {"\n", "\r\n", " "};

    private const StringSplitOptions
        splitFlags = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    private readonly UrlProviderService provider;
    private bool isEphemeral = true;

    public PrivateLinkModule(IServiceProvider serviceProvider) {
        LogDebug(GetType(), "Initializing");
        // We have to do this because DI doesn't work properly when we're started by the interaction handler...
        provider = serviceProvider.GetRequiredConcreteService<UrlProviderService, ServiceBase>();
        LogDebug(GetType(), $"Child of {provider.Epoch}");
    }

    private IEnumerable<string> IterateWords(string str) => str.Split(splitTokens, splitFlags).Distinct();

    [SlashCommand("privatize", "Post a private version of a link"), UsedImplicitly]
    public async Task PostPrivateLink_Slash(string url) {
        var servicedUrl = provider.GetLinkFromRandomApplicableService(url);

        if (servicedUrl is null) {
            await RespondAsync("No service found to handle the given link.", ephemeral: true);
            return;
        }

        await RespondAsync(servicedUrl, ephemeral: false);
    }

    [MessageCommand("Show private link"), UsedImplicitly]
    public async Task ShowPrivateLink_Msg(IMessage msg) {
        LogInfo("/privatize",
            $"Providing private link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");
        foreach (var word in IterateWords(msg.Content)) {
            var servicedUrl = provider.GetLinkFromRandomApplicableService(word);

            if (servicedUrl is null) {
                continue;
            }

            await RespondAsync(servicedUrl, ephemeral: isEphemeral);
            return;
        }

        await RespondAsync("No service found to handle the given link.", ephemeral: true);
    }

    [MessageCommand("Post private link"), UsedImplicitly]
    public async Task PostPrivateLink_Msg(IMessage msg) {
        isEphemeral = false;
        await ShowPrivateLink_Msg(msg);
        isEphemeral = true;
    }

    [SlashCommand("archive", "Archive a link"), UsedImplicitly]
    public async Task ArchiveLink_Slash(string url) {
        LogInfo("/archive",
            $"Providing archive link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl)) {
            await RespondAsync("The url you provided doesn't appear to be valid");
            return;
        }

        url = parsedUrl.ToString();
        await RespondAsync("Please wait while I check available archives (this may take up to a minute)");
        var httpClientHandler = new HttpClientHandler {AllowAutoRedirect = false};
        var httpClient = new HttpClient(httpClientHandler) {
            Timeout = TimeSpan.FromSeconds(60)
        };
        // RIP timegate
        // var requestUrl = new Uri(@"http://timetravel.mementoweb.org/timegate/" + url);
        var requestUrl = new Uri($@"https://web.archive.org/web/9999/{url}");
        var httpMessage = new HttpRequestMessage(HttpMethod.Head, requestUrl);
        string? redirectUrl = null;

        // See if it exists in archived form
        try {
            var redirectData = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead);
            var location = redirectData.Headers.Location;
            // Decline timegate-to-timegate redirects
            if (location is not null) {
                // Timegate needs && location.Authority != requestUrl.Authority
                redirectUrl = location.ToString();
            }
        }
        catch {
            // redirectUrl will be null anyway
        }

        if (redirectUrl is not null) {
            LogInfo("/archive", "Found an archived copy");
            await ModifyOriginalResponseAsync(m =>
                m.Content = $"`{url}`\n{redirectUrl}"
            );
            return;
        }

        // Our redirect has failed, let's ask archive for a copy
        try {
            await ModifyOriginalResponseAsync(m =>
                m.Content = "No pre-existing archive found, please wait while I make a new one"
            );
            httpMessage = new HttpRequestMessage(HttpMethod.Head, @"https://web.archive.org/save/" + url);
            var redirectData = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead);
            redirectUrl = redirectData.Headers.Location?.ToString();
        }
        catch (Exception e) {
            LogWarning("/archive", $"HTTP Exception: {e.Message,20}");
            // sigh
        }

        if (redirectUrl is not null) {
            LogInfo("/archive", "Providing a new archive");
            await ModifyOriginalResponseAsync(m =>
                m.Content = $"`{url}`\n{redirectUrl}"
            );
            return;
        }

        LogInfo("/archive", "Couldn't archive given link");
        await ModifyOriginalResponseAsync(m =>
            m.Content =
                $"Archiving `{url}` failed. Make sure your link is valid and publicly accessible.\nPlease also note that archive.org may be overloaded with requests at peak times."
        );
    }

    [MessageCommand("Post archived link"), UsedImplicitly]
    public async Task ArchiveLink_Msg(IMessage msg) {
        string? url = null;
        foreach (var word in IterateWords(msg.Content)) {
            if (!Uri.TryCreate(word, UriKind.Absolute, out var result)) {
                continue;
            }

            url = result.ToString();
            break;
        }

        if (url is null) {
            await RespondAsync("No link found in the given message.");
            return;
        }

        await ArchiveLink_Slash(url);
    }

    [SlashCommand("help", "Request a list of supported services and commands"), UsedImplicitly]
    public async Task Help_Slash() {
        var serviceDescriptions = string.Empty;
        foreach (var service in provider.Providers.Values) {
            // No dynamic instances to be blacklisted
            if (service.PrimaryUrls is null || service.PrimaryUrls.Length == 0) {
                serviceDescriptions +=
                    $"\n{service.FriendlyName} ({service.SecondaryUrls.Length} instances, 0 of which are down)";
                continue;
            }

            var activeCount = service.PrimaryUrls!.Length;
            var inactiveCount = activeCount - service.PrimaryUrls!.Except(provider.Blacklist).Count();
            serviceDescriptions +=
                $"\n{service.FriendlyName} ({activeCount} instances, {inactiveCount} of which are down)";
        }

        await RespondAsync(
            $@"Currently providing links for:{serviceDescriptions}

Showing a link will send it privately, posting it will send it publicly.
You may archive a link to ask for a historical copy instead of a proxy.

Press `[...] -> Apps` on a message to get started.");
    }
}