// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Misc.CLog;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace InvaderZim.Misc;

/**
 * Information how the log verbosity is represented in logger (e.g., text color or tag).
 */
public struct SLogVerbosityInfo()
{
	/** Specifies a tag for log verbosity inside []. */
	public string Tag { get; init; } = "NONE";

	/** Specifies colors for background and text. */
	public ConsoleColor TextColor { get; init; } = ConsoleColor.White;

	public ConsoleColor BackgroundColor { get; } = ConsoleColor.Black;
};

// TODO: Move to Microsoft.Extensions.Logging
public static class CLog
{
	public const LogLevel MinimumLogLevel = LogLevel.Information;
	
	// public static void LogCritical(string Text) { Log(LogLevel.Critical, Text); }
	public static void LogError(string Text) { Log(LogLevel.Error, Text); }
	public static void LogWarning(string Text) { Log(LogLevel.Warning, Text); }
	public static void LogInfo(string Text) { Log(LogLevel.Information, Text); }
	public static void LogDebug(string Text) { Log(LogLevel.Debug, Text); }
	public static void LogTrace(string Text) { Log(LogLevel.Trace, Text); }

	/** Logs a message to console. */
	private static void Log(LogLevel LogVerbosity, string Message)
	{
		if (LogVerbosity < MinimumLogLevel) return;
		
		if (!VerbosityInfo.TryGetValue(LogVerbosity, out SLogVerbosityInfo CurrentVerbosityInfo))
		{
			System.Diagnostics.Debug.Assert(false);
		}
		
		string Timestamp = DateTimeOffset.Now.ToString("MMM dd yyyy - hh:mm:ss tt");
		Int32 ThreadId = Environment.CurrentManagedThreadId;
		string Thread = $"{ThreadId}".PadRight(3);
		string Category = $"{Thread} /Gir".PadRight(17);
		
		Console.Write($"[{Timestamp}] [{Category}] ");
			
		Console.ForegroundColor = CurrentVerbosityInfo.TextColor;
		Console.BackgroundColor = CurrentVerbosityInfo.BackgroundColor;
		Console.Write($"[{CurrentVerbosityInfo.Tag}]");
			
		Console.ResetColor();
		Console.WriteLine($" {Message}");
	}

	/** Maps each log verbosity level to its corresponding formatting information. */
	private static readonly Dictionary<LogLevel, SLogVerbosityInfo> VerbosityInfo = new() 
	{
		{ LogLevel.Critical,    new SLogVerbosityInfo {Tag = "Crit ", TextColor = ConsoleColor.Red} },
		{ LogLevel.Error,       new SLogVerbosityInfo {Tag = "Error", TextColor = ConsoleColor.Red} },
		{ LogLevel.Warning,     new SLogVerbosityInfo {Tag = "Warn ", TextColor = ConsoleColor.Yellow} },
		{ LogLevel.Information, new SLogVerbosityInfo {Tag = "Info ", TextColor = ConsoleColor.DarkCyan} },
		{ LogLevel.Debug,       new SLogVerbosityInfo {Tag = "Debug", TextColor = ConsoleColor.DarkMagenta} },
		{ LogLevel.Trace,       new SLogVerbosityInfo {Tag = "Trace", TextColor = ConsoleColor.DarkMagenta} }
	};
}
