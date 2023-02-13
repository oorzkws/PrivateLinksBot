// using Discord.WebSocket;
//
// namespace PrivateLinksBot.UrlProvider;
//
// public class PipedUrlProvider : BaseYoutubeProvider
// {
//     public PipedUrlProvider(DiscordSocketClient client) : base(client)
//     {
//         ServiceName = "YouTube - Piped";
//         ServiceUrls = UrlProviderBroker.ServiceData?["piped"]["normal"] ?? new[] {"https://piped.video"};
//     }
// }