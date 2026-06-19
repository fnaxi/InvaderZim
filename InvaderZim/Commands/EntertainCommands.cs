// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InvaderZim.Misc;

namespace InvaderZim.Commands;

public class CEntertainCommands : BaseCommandModule
{
	[Group("feed")]
	public class CFeed : BaseCommandModule
	{
		[Command("waffles")]
		[Description("Gifts zim some tasty waffles to eat")]
		public async Task Waffles(CommandContext Context)
		{
			Debug.Assert(Context.Member != null);
			await Context.RespondAsync($"{Context.Member.Mention} {RandomString(CQuote.Waffles)}");
		}
	
		[Command("tacos")]
		[Description("Gifts zim some tacos to eat")]
		public async Task Tacos(CommandContext Context)
		{
			Debug.Assert(Context.Member != null);
			await Context.RespondAsync($"{Context.Member.Mention} {RandomString(CQuote.Tacos)}");
		}
	}
}
