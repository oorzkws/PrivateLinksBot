﻿using Discord.WebSocket;

namespace PrivateLinksBot;

/// <summary>
/// Example service doing basic things
/// </summary>
public class ExampleService : ServiceBase
{
    public ExampleService(DiscordSocketClient client) : base(client)
    {
    }

    public override async Task InitializeAsync()
    {
        await Logger.LogInfo($"ExampleService started, client status: {Client.ConnectionState}");
        await base.InitializeAsync();
    }
}