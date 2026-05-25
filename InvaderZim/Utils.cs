// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.CUtils;
global using static InvaderZim.CAssert;
using System.Diagnostics;
using DSharpPlus.Entities;

namespace InvaderZim;

public class CUtils
{
	/** Inverts String.IsNullOrWhiteSpace for cleaner checks. */
	public static bool IsTextValid(string InText)
	{
		return !String.IsNullOrWhiteSpace(InText);
	}
	
	public static List<T> Concat<T>(List<T> InList1, List<T> InList2)
	{
		return InList1.Concat(InList2).ToList();
	}

	public static DiscordColor YellowGreen = new DiscordColor("#9ACD32");
}

public class CAssert
{
	/**
	 * Checks a condition and halts execution if it's false.
	 * <param name="bCondition"> The condition to check. </param>
	 * <param name="Message"> Optional message to log when the assertion fails. </param>
	 */
	public static bool Verify(bool bCondition, string Message = "")
	{
		if (bCondition) return true;
		
		CLog.Error($"Assertion failed! {Message}");
		CLog.Error("Stack Trace: \n" + new StackTrace(2));
#if DEBUG
		Debug.Assert(false, Message);
#else
		Environment.Exit(1);
#endif

		return false;
	}

	/** Used for pieces of code which should never be executed. */
	public static bool CheckNoEntry(string Message = "")
	{
		return Verify(false, IsTextValid(Message) ? Message : "This piece of code should never be executed!");
	}
}
