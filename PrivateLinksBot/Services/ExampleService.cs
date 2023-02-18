using Discord.WebSocket;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

/// <summary>
/// Example service doing basic things
/// </summary>
public class ExampleService : ServiceBase {
    public ExampleService(DiscordSocketClient service) : base(service) {
    }

    public override async Task InitializeAsync() {
        var timeOfDay = DateTime.Now.Hour < 12 ? "Morning" : DateTime.Now.Hour < 18 ? "Afternoon" : "Evening";
        LogVerbose("DemoService", $"Good {timeOfDay}");
        await base.InitializeAsync();
    }
}