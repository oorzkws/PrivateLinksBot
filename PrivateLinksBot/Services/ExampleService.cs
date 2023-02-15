using Discord.WebSocket;

namespace PrivateLinksBot;

/// <summary>
/// Example service doing basic things
/// </summary>
public class ExampleService : ServiceBase {
    public ExampleService(DiscordSocketClient service) : base(service) {
    }

    public override async Task InitializeAsync() {
        await Logger.LogInfo($"ExampleService started, client status: {Client.ConnectionState}");
        await base.InitializeAsync();
    }
}