using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using As = Crayon.Output;

namespace PrivateLinksBot; 

public static class ConsoleLoggerExtensions {
    public static ILoggingBuilder AddCustomFormatter(this ILoggingBuilder builder, LogLevel logLevel = LogLevel.Trace){
        return builder
            .AddConsole(options => options.FormatterName = nameof(CustomLoggingFormatter))
            .SetMinimumLevel(logLevel)
            .AddConsoleFormatter<CustomLoggingFormatter, ConsoleFormatterOptions>();
    }
}

[UsedImplicitly]
public sealed class CustomLoggingFormatter : ConsoleFormatter, IDisposable {
    private readonly IDisposable reloadToken;
    private ConsoleFormatterOptions formatOptions;

    public CustomLoggingFormatter(IOptionsMonitor<ConsoleFormatterOptions> options) : base(nameof(CustomLoggingFormatter)) {
        reloadToken = options.OnChange(ReloadOptions)!;
        formatOptions = options.CurrentValue;
    }

    private void ReloadOptions(ConsoleFormatterOptions options) => formatOptions = options;

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider,
        TextWriter textWriter) {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null) {
            return;
        }

        message = logEntry.LogLevel switch {
            LogLevel.Critical => As.Black(message),
            LogLevel.Trace => As.Dim().Blue(message),
            LogLevel.Error => As.Dim().Magenta(message),
            LogLevel.Information => As.White(message),
            LogLevel.Debug => As.Dim().Green(message),
            LogLevel.Warning => As.Dim().Yellow(message),
            _ => As.White(message) // CS8524 - enums take arbitrary int casts
        };

        var dateString = As.Bold().White($"{DateTime.Now:HH:mm:ss}");
        var eventString = As.Underline(logEntry.EventId.Name ?? "");
        textWriter.WriteLine($"{dateString} {eventString,-20} {message,0}");
    }

    public void Dispose() {
        reloadToken.Dispose();
    }
}