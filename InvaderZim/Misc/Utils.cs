// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Misc.CUtils;
using DSharpPlus.Entities;

namespace InvaderZim.Misc;

public static class CUtils
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

	public static DiscordColor YellowGreen = new("#9ACD32");
	
	private static readonly Random RandomSeed = new();
	public static string RandomString(List<string> Options)
	{
		Int32 Index = RandomSeed.Next(Options.Count);
		return Options[Index];
	}
}
