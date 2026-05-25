// CopyRight https://github.com/fnaxi. All Rights Reserved.

namespace InvaderZim;

public enum ELogVerbosity
{
	/** Used to disable logging. */
	NoLogging = 0,
	
	/** Prints an error to console and log file. */
	Error = 1,
	
	/** Prints a warning to console and log file. */
	Warning = 2,
	
	/** Prints a message to console and log file. */
	Info = 3,
	
	/** Prints a verbose message to console and log file. */
	Verbose = 4,
	
	/** Enable all logging verbosity. */
	All = 5
}

/**
 * Information how the log verbosity is represented in logger (e.g., text color or tag).
 */
public struct SLogVerbosityInfo
{
	public SLogVerbosityInfo()
	{
		Tag = "NONE";
		TextColor = ConsoleColor.White;
		BackgroundColor = ConsoleColor.Black;
	}

	/** Specifies a tag for log verbosity inside []. */
	public string Tag { get; set; }

	/** Specifies colors for background and text. */
	public ConsoleColor TextColor { get; set; }

	public ConsoleColor BackgroundColor { get; set; }
};

// TODO: Move to Microsoft.Extensions.Logging
public static class CLog
{
	/** Wrappers of Log() method for different log verbosity. */
	public static void Error(string Text) { Log(ELogVerbosity.Error, Text); }
	public static void Warning(string Text) { Log(ELogVerbosity.Warning, Text); }
	public static void Info(string Text) { Log(ELogVerbosity.Info, Text); }
	public static void Verbose(string Text) { Log(ELogVerbosity.Verbose, Text); }

	public static void Status(Int32 ExitCode, string Text)
	{
		Log(ExitCode == 0 ? ELogVerbosity.Info : ELogVerbosity.Error, Text);
	}

	/** Logs a message to console. */
	private static void Log(ELogVerbosity LogVerbosity, string Message)
	{
		if (LogVerbosity > MaxLogVerbosity) return;
		
		if (!VerbosityInfo.TryGetValue(LogVerbosity, out SLogVerbosityInfo CurrentVerbosityInfo))
		{
			CheckNoEntry();
		}
		
		string Timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
		Int32 ThreadId = Environment.CurrentManagedThreadId;
		string Thread = $"{ThreadId}".PadRight(3);
		string Category = $"{Thread} /InvaderZim".PadRight(17);
		
		Console.Write($"[{Timestamp}] [{Category}] ");
			
		Console.ForegroundColor = CurrentVerbosityInfo.TextColor;
		Console.BackgroundColor = CurrentVerbosityInfo.BackgroundColor;
		Console.Write($"[{CurrentVerbosityInfo.Tag}]");
			
		Console.ResetColor();
		Console.WriteLine($" {Message}");
	}

	/** Determines max log verbosity that is allowed to be logged. */
	private static readonly ELogVerbosity MaxLogVerbosity = ELogVerbosity.All;

	/** Maps each log verbosity level to its corresponding formatting information. */
	private static readonly Dictionary<ELogVerbosity, SLogVerbosityInfo> VerbosityInfo = new() 
	{
		{ ELogVerbosity.Error,   new SLogVerbosityInfo {Tag = "Error", TextColor = ConsoleColor.Red} },
		{ ELogVerbosity.Warning, new SLogVerbosityInfo {Tag = "Warn ",  TextColor = ConsoleColor.Yellow} },
		{ ELogVerbosity.Info,    new SLogVerbosityInfo {Tag = "Info ",   TextColor = ConsoleColor.DarkCyan} },
		{ ELogVerbosity.Verbose, new SLogVerbosityInfo {Tag = "Info ",   TextColor = ConsoleColor.Cyan} }
	};
}
