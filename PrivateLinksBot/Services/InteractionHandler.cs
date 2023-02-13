using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace PrivateLinksBot;

public class InteractionHandler : ServiceBase
{
    private InteractionService Handler { get; }
    private IServiceProvider Provider { get; }
    
    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider provider) : base(client)
    {
        Handler = handler;
        Provider = provider;
    }
    
    public override async Task InitializeAsync()
    {
        //Client.Ready += ReadyAsync;
        Client.GuildAvailable += guild => Handler.RegisterCommandsToGuildAsync(guild.Id);
        Handler.Log += Logger.LogAsync;
        
        await Handler.AddModulesAsync(Assembly.GetExecutingAssembly(), Provider);
        await Logger.LogInfo($"InteractionHandler started");

        Client.InteractionCreated += HandleInteraction;
        
        await base.InitializeAsync();
    }

    // ref: https://github.com/discord-net/Discord.Net/blob/dev/samples/InteractionFramework/InteractionHandler.cs
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create a context within the interaction, then execute in the registered service provider
            var executionContext = new SocketInteractionContext(Client, interaction);
            var result = await Handler.ExecuteCommandAsync(executionContext, Provider);
            if (!result.IsSuccess && result.Error is InteractionCommandError.UnmetPrecondition)
            {
                // Do something about it
            }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}