using Discord;

namespace PrivateLinksBot;

/// <summary>
/// Handles logging to console
/// </summary>
public static class Logger {
    private static readonly Dictionary<LogSeverity, ConsoleColor> colorMap = new() {
        {LogSeverity.Debug, ConsoleColor.DarkBlue},
        {LogSeverity.Verbose, ConsoleColor.Gray},
        {LogSeverity.Info, ConsoleColor.White},
        {LogSeverity.Warning, ConsoleColor.DarkYellow},
        {LogSeverity.Error, ConsoleColor.DarkMagenta},
        {LogSeverity.Critical, ConsoleColor.DarkRed}
    };

    private static readonly object writeLock = new object();

    private static ConsoleColor originalForeground = ConsoleColor.White;
    private static ConsoleColor originalBackground = ConsoleColor.Black;

    public static LogSeverity MinimumLogSeverity = LogSeverity.Info;


    private static void SetColors(ConsoleColor? fore, ConsoleColor? back) {
        originalBackground = Console.BackgroundColor;
        if (back.HasValue)
            Console.BackgroundColor = back.Value;

        originalForeground = Console.ForegroundColor;
        if (fore.HasValue)
            Console.ForegroundColor = fore.Value;
    }

    private static void ResetColors() {
        if (Console.ForegroundColor != originalForeground)
            Console.ForegroundColor = originalForeground;

        if (Console.BackgroundColor != originalBackground)
            Console.BackgroundColor = originalBackground;
    }

    private static void WriteLog(
        LogMessage msg,
        ConsoleColor? foregroundColor,
        ConsoleColor? backgroundColor
    ) {
        lock (writeLock) {
            SetColors(foregroundColor, backgroundColor);

            Console.WriteLine(msg.ToString());

            ResetColors();
        }
    }

    public static async Task LogAsync(LogMessage message) {
        if (message.Severity <= MinimumLogSeverity) {
            WriteLog(message, colorMap[message.Severity], null);
        }

        await Task.CompletedTask;
    }

    public static Task LogDebug(string message) => LogAsync(new LogMessage(LogSeverity.Debug, "Logger", message));
    public static Task LogVerbose(string message) => LogAsync(new LogMessage(LogSeverity.Verbose, "Logger", message));
    public static Task LogInfo(string message) => LogAsync(new LogMessage(LogSeverity.Info, "Logger", message));
    public static Task LogWarning(string message) => LogAsync(new LogMessage(LogSeverity.Warning, "Logger", message));
    public static Task LogError(string message) => LogAsync(new LogMessage(LogSeverity.Error, "Logger", message));
    public static Task LogCritical(string message) => LogAsync(new LogMessage(LogSeverity.Critical, "Logger", message));
}