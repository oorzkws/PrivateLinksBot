using System.Diagnostics.CodeAnalysis;
using Discord;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace PrivateLinksBot;

/// <summary>
/// Handles logging to console
/// </summary>
[SuppressMessage("Usage", "CA2254:Template should be a static expression"), UsedImplicitly] // Enforced in receiving method
public class Logger {
    private static readonly ILogger iLog;

    static Logger() {
        iLog = LoggerFactory.Create(
            builder => builder.AddCustomFormatter(LogLevel.Debug)
        ).CreateLogger(string.Empty);
    }

    public static void WriteLog(LogMessage msg) {
        WriteLog(msg.Severity switch {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Debug => LogLevel.Trace,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Warning => LogLevel.Warning,
            _ => LogLevel.Debug // CS8524 - enums take arbitrary int casts
        }, msg.Source, msg.Exception, msg.Message);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void WriteLog(
        LogLevel level,
        string source,
        Exception? exception,
        [StructuredMessageTemplate] string? message,
        params object[] args
    ) {
        iLog.Log(level, new EventId(-1, source), exception, message, args);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void WriteLog(
        LogLevel level,
        string source,
        [StructuredMessageTemplate] string? message,
        params object[] args
    ) {
        iLog.Log(level, new EventId(-1, source), message, args);
    }

    // ReSharper disable UnusedMember.Global
    public static void LogDebug(string source, string message) => WriteLog(LogLevel.Trace, source, message);
    public static void LogVerbose(string source, string message) => WriteLog(LogLevel.Debug, source, message);
    public static void LogInfo(string source, string message) => WriteLog(LogLevel.Information, source, message);
    public static void LogWarning(string source, string message) => WriteLog(LogLevel.Warning, source, message);
    public static void LogError(string source, string message) => WriteLog(LogLevel.Error, source, message);
    public static void LogCritical(string source, string message) => WriteLog(LogLevel.Critical, source, message);

    public static void LogDebug(Type source, string message) => WriteLog(LogLevel.Trace, source.Name, message);
    public static void LogVerbose(Type source, string message) => WriteLog(LogLevel.Debug, source.Name, message);
    public static void LogInfo(Type source, string message) => WriteLog(LogLevel.Information, source.Name, message);
    public static void LogWarning(Type source, string message) => WriteLog(LogLevel.Warning, source.Name, message);
    public static void LogError(Type source, string message) => WriteLog(LogLevel.Error, source.Name, message);
    public static void LogCritical(Type source, string message) => WriteLog(LogLevel.Critical, source.Name, message);
    // ReSharper restore UnusedMember.Global
}