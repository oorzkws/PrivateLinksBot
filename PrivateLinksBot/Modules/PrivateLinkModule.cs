using Discord;
using Discord.Interactions;
using PrivateLinksBot.UrlProvider;

namespace PrivateLinksBot;

public class PrivateLinkModule : InteractionModuleBase {
    private readonly string[] splitTokens = new[] {"\n", "\r\n", " "};
    private readonly StringSplitOptions splitFlags = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    private bool isEphemeral = true;

    private IEnumerable<string> IterateWords(string str) {
        return str.Split(splitTokens, splitFlags).Distinct();
    }

    private string? PrivatizeLink(string url) {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) ? UrlProviderBroker.GetNewUrlFromRandomService(result.ToString()) : null;
    }

    [SlashCommand("privatize", "Post a private version of a url")]
    public async Task Privatize(string url)
    {
        var servicedUrl = PrivatizeLink(url);

        if (servicedUrl is null) {
            await RespondAsync("No service found to handle the given url.", ephemeral: true);
            return;
        }

        await RespondAsync(servicedUrl, ephemeral: false);
    }
    
    [MessageCommand("Show private link")]
    public async Task Interact(IMessage msg) {
        await Logger.LogInfo(
            $"Providing private link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");
        foreach (var word in IterateWords(msg.Content)) {
            var servicedUrl = PrivatizeLink(word);

            if (servicedUrl is null)
                continue;

            await RespondAsync(servicedUrl, ephemeral: isEphemeral);
            return;
        }

        await RespondAsync("No service found to handle the given url.", ephemeral: true);
    }

    [MessageCommand("Post private link")]
    public async Task GetAndPost(IMessage msg) {
        isEphemeral = false;
        await Interact(msg);
        isEphemeral = true;
    }
    
    [SlashCommand("archive", "Archive a url")]
    public async Task Archive(string url)
    {
        await Logger.LogInfo(
            $"Providing archive link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl)) {
            await RespondAsync("The url you provided doesn't appear to be valid");
            return;
        }

        url = parsedUrl.ToString();
        await RespondAsync("Please wait while I check available archives (this may take up to a minute)");
        var httpClientHandler = new HttpClientHandler {AllowAutoRedirect = false};
        var httpClient = new HttpClient (httpClientHandler) {
            Timeout = TimeSpan.FromSeconds(60)
        };
        // RIP timegate
        // var requestUrl = new Uri(@"http://timetravel.mementoweb.org/timegate/" + url);
        var requestUrl = new Uri($@"https://web.archive.org/web/{DateTime.Today:yyyyMMdd}/{url}");
        var httpMessage = new HttpRequestMessage(HttpMethod.Head, requestUrl);
        string? redirectUrl = null;
        
        // See if it exists in archived form
        try {
            await Logger.LogInfo($"\tAsking {requestUrl}");
            var redirectData = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead);
            var location = redirectData.Headers.Location;
            // Decline timegate-to-timegate redirects
            if (location is not null) { // Timegate needs && location.Authority != requestUrl.Authority
                redirectUrl = location.ToString();
            }
        }
        catch {
            await Logger.LogInfo("\tError during httprequest");
            // redirectUrl will be null anyway
        }

        if (redirectUrl is not null) {
            await Logger.LogInfo("\tFound an archived copy");
            await ModifyOriginalResponseAsync(m =>
                m.Content = $"`{url}`\n{redirectUrl}"
            );
            return;
        }

        // Our redirect has failed, let's ask archive for a copy
        try {
            httpMessage = new HttpRequestMessage(HttpMethod.Head, @"https://web.archive.org/save/" + url);
            var redirectData = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead);
            redirectUrl = redirectData.Headers.Location?.ToString();
            await Logger.LogInfo(redirectData.ToString());
            await Logger.LogInfo(redirectData.StatusCode.ToString());
        }
        catch {
            // sigh
        }
        
        if (redirectUrl is not null) {
            await Logger.LogInfo("\tProviding a new archive");
            await ModifyOriginalResponseAsync(m =>
                m.Content = $"`{url}`\n{redirectUrl}"
            );
            return;
        }
        
        await Logger.LogInfo("\tCouldn't archive given link");
        await ModifyOriginalResponseAsync(m => 
            m.Content = $"Archiving `{url}` failed. Make sure your link is valid and publicly accessible."
        );
    }
    
    [MessageCommand("Post archived link")]
    public async Task ArchiveMessage(IMessage msg) {
        string? url = null;
        foreach (var word in IterateWords(msg.Content)) {
            if (Uri.TryCreate(word, UriKind.Absolute, out var result)) {
                url = result.ToString();
                break;
            }
        }

        if (url is null) {
            await RespondAsync("No link found in the given message.");
            return;
        }

        await Archive(url);
    }

    [SlashCommand("help", "Request a list of supported services and commands")]
    public async Task SendHelp() {
        var services = string.Empty;
        foreach (var service in UrlProviderBroker.UrlProviderBases) {
            var baseList = UrlProviderBroker.ServiceData[service.ServiceName];
            var cleanList = baseList.Except(UrlProviderBroker.UrlBlacklist).ToList();
            services +=
                $"\n{service.ServiceNameFriendly} ({baseList.Length} instances, {baseList.Length - cleanList.Count} of which are down)";
        }

        await RespondAsync(
$@"Currently providing links for:{services}

Showing a link will send it privately, posting it will send it publicly.
You may archive a link to ask for a historical copy instead of a proxy.

Press `[...] -> Apps` on a message to get started.");
    }
}