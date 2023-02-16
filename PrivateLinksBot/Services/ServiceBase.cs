using Discord.WebSocket;

namespace PrivateLinksBot;

/// <summary>
/// Abstract base for any sort of service
/// </summary>
public abstract class ServiceBase {
    protected DiscordSocketClient Client { get; }

    // ReSharper disable once PublicConstructorInAbstractClass - Required for DI
    public ServiceBase(DiscordSocketClient client) {
        Logger.LogDebug($"Constructing {GetType()}");
        Client = client;
    }

    public virtual Task InitializeAsync() {
        return Task.CompletedTask;
    }
}