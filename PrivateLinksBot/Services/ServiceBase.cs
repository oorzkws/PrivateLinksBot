using Discord.WebSocket;
using JetBrains.Annotations;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

/// <summary>
/// Abstract base for any sort of service
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ServiceBase {
    protected DiscordSocketClient Client { get; }
    public readonly long Epoch;

    // ReSharper disable once PublicConstructorInAbstractClass - Required for DI
    public ServiceBase(DiscordSocketClient client) {
        Client = client;
        Epoch = DateTime.Now.Ticks;
        LogDebug(GetType(), $"Constructing, ID {Epoch}");
    }

    public virtual Task InitializeAsync() {
        LogDebug(GetType(), $"Finished async init for ID {Epoch}");
        return Task.CompletedTask;
    }
}