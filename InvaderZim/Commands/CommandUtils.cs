// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Commands.CCommandUtils;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim.Commands;

public static class CCommandUtils
{
	public const Int32 TemporaryResponseTime = 5; // in seconds
	
	public static async Task NoRights(CommandContext Context)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "You Earth creatures are strange and smelly",
			Description = $"You're not the one who can ask his honor zim{CEmoji.GirBlep} for something like that!",
			Color = YellowGreen
		};
		await Context.Channel.SendMessageAsync(Embed);
	}
	
	public static bool SentInBotChannel(CommandContext Context)
	{
		return Context.Channel.Id is not (CChannel.BotChat or CChannel.Test);
	}

	public static async Task<bool> IsTargetingBotOrSelf(CommandContext Context, DiscordMember Member)
	{
		Debug.Assert(Context.Member != null);
		if (Member.Id == Context.Client.CurrentUser.Id || Member.Id == Context.Member.Id)
		{
			await Context.RespondAsync($"{RandomString(CQuote.BotOrSelfBan)} {CEmoji.GirDance}");
			return true;
		}

		return false;
	}

	public static bool IsContextChannel(CommandContext Context, ref DiscordChannel? Channel)
	{
		if (Channel == null)
		{
			Channel = Context.Channel;
			return true;
		}
		return false;
	}
	
	public static TimeSpan ParseTime(string Input)
	{
		TimeSpan Time = TimeSpan.Zero;
		
		MatchCollection Matches = Regex.Matches(Input.ToLower(), @"(\d+)([dhms])");
		foreach (Match CurrentMatch in Matches)
		{
			Int32 Value = Int32.Parse(CurrentMatch.Groups[1].Value);
			string Unit = CurrentMatch.Groups[2].Value;

			Time += Unit switch
			{
				"d" => TimeSpan.FromDays(Value),
				"h" => TimeSpan.FromHours(Value),
				"m" => TimeSpan.FromMinutes(Value),
				"s" => TimeSpan.FromSeconds(Value),
				_   => TimeSpan.Zero
			};
		}
		
		return Time;
	}
}
