// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Misc.CUtils;
using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using InvaderZim.ID;

namespace InvaderZim.Misc;

public static class CUtils
{
	/*----------------------------------------------------------------------------
		Discord API
	----------------------------------------------------------------------------*/

	public static bool CanModerate(DiscordMember? Member)
	{
		Debug.Assert(Member != null);
		bool bAdmin = Member.Permissions.HasFlag(Permissions.Administrator);

		return bAdmin || Member.Roles.Any(r => r.Id is CRole.Admin or CRole.Moderator);
	}
	
	/*----------------------------------------------------------------------------
		Misc
	----------------------------------------------------------------------------*/
	
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
