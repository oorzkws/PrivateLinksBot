using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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
        Logger.MinimumLogSeverity = LogSeverity.Debug;
        // Build the client service
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        client.Log += Logger.LogAsync;

        // Activate add-on services
        await serviceProvider.GetRequiredService<ServiceHandler.ServiceInterface>()
            .ActivateAsync();

        var token = Environment.GetEnvironmentVariable(tokenEnvironmentVariable);

        if (token is null)
            throw new ApplicationException(
                $"No token found. Please set the {tokenEnvironmentVariable} environment variable.");

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Wait until program closed
        await Task.Delay(-1);
    }

    static IServiceProvider CreateServices() {
        var socketConfig = new DiscordSocketConfig() {
            // All minus unused privileged intents
            GatewayIntents = GatewayIntents.All ^
                             GatewayIntents.GuildScheduledEvents ^
                             GatewayIntents.GuildPresences ^
                             GatewayIntents.GuildInvites
        };

        var serviceCollection = new ServiceCollection()
            .AddSingleton(socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionService>()
            .RegisterImplicitServices();

        return serviceCollection.BuildServiceProvider();
    }
}