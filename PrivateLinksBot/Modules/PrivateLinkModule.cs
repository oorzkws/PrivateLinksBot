using Discord;
using Discord.Commands;
using Discord.Interactions;
using PrivateLinksBot.UrlProvider;

namespace PrivateLinksBot;

public class PrivateLinkModule : InteractionModuleBase {
    private string[] splitTokens = new[] {"\n", "\r\n", " "};
    private StringSplitOptions splitFlags = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    private bool isEphemeral = true;

    private IEnumerable<string> IterateWords(string str) {
        return str.Split(splitTokens, splitFlags).Distinct();
    }

    [MessageCommand("Show private link")]
    public async Task Interact(IMessage msg) {
        await Logger.LogInfo(
            $"Providing private link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");
        foreach (var word in IterateWords(msg.Content)) {
            if (!Uri.IsWellFormedUriString(word, UriKind.RelativeOrAbsolute))
                continue;

            var servicedUrl = UrlProviderBroker.RequestUrl(word);

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
    }

    /*
    [SlashCommand("privatize", "Request a private version of your url")]
    public async Task Privatize([Discord.Interactions.Summary("url to privatize")] string url)
    {
        await Logger.LogInfo($"Providing private link for {Context.User.Username} in #{Context.Channel.Name} @ {Context.Guild.Name}");

        if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            return
        }
    }*/

    [SlashCommand("help", "Request a list of supported services")]
    public async Task SendHelp() {
        var services = string.Empty;
        foreach (var service in UrlProviderBroker.UrlProviderBases) {
            var baseList = UrlProviderBroker.ServiceData[service.ServiceName];
            var cleanList = baseList.Except(UrlProviderBroker.UrlBlacklist).ToList();
            services +=
                $"\n{service.ServiceNameFriendly} ({baseList.Length} instances, {baseList.Length - cleanList.Count} of which are down)";
        }

        await RespondAsync(
            $"Currently providing links for:{services}\n\nPress `[...] -> Apps` on a message to get started.");
    }
}