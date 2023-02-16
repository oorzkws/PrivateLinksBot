using Discord.WebSocket;

namespace PrivateLinksBot;

/// <summary>
/// Abstract base for any sort of service
/// </summary>
public abstract class ServiceBase {
    protected DiscordSocketClient Client { get; }

    public ServiceBase(DiscordSocketClient client) {
        this.Client = client;
    }

    public virtual Task InitializeAsync() {
        return Task.CompletedTask;
    }
}