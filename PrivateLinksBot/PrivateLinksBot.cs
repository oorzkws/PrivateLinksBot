using Discord;
using Discord.Logging;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting.Systemd;
using static PrivateLinksBot.Logger;

namespace PrivateLinksBot;

/// <summary>
/// Main bot class, handles init and calls services
/// </summary>
public class PrivateLinksBot {
    private const string tokenEnvironmentVariable = "PrivateLinksBotToken";
    private readonly IServiceProvider serviceProvider;

    public PrivateLinksBot() {
        serviceProvider = CreateServices();
    }

    public async Task RunAsync() {
        // Build the client service
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        client.Log += msg => {
            WriteLog(msg);
            return Task.CompletedTask;
        };

        // Activate add-on services
        await serviceProvider.ActivateAsync<ServiceBase>();

        var token = Environment.GetEnvironmentVariable(tokenEnvironmentVariable);

        if (token is null)
            throw new ApplicationException(
                $"No token found. Please set the {tokenEnvironmentVariable} environment variable.");

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Wait until program closed
        await Task.Delay(-1);
    }

    private static IServiceProvider CreateServices() {
        var socketConfig = new DiscordSocketConfig {
            // All minus unused privileged intents
            GatewayIntents = GatewayIntents.All ^
                             GatewayIntents.GuildScheduledEvents ^
                             GatewayIntents.GuildPresences ^
                             GatewayIntents.GuildInvites
        };

        var serviceCollection = new ServiceCollection()
            .AddSingleton(LoggerFactory.Create(b => b.AddCustomFormatter()))
            .AddSystemd()
            .AddSingleton(socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()))
            .RegisterImplicitServices<ServiceBase>();

        return serviceCollection.BuildServiceProvider();
    }
}