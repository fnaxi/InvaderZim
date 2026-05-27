// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim.Commands;

public class CModerationCommands : BaseCommandModule
{
	private const Int32 TemporaryResponseTime = 5; // in seconds
	
	[Command("mute")]
	[Description("Mutes the specified member")]
	public async Task Mute(CommandContext Context, 
		[Description("The member to mute")] DiscordMember Member,
		[Description("Time of the mute in minutes")] UInt32 Duration = 30, // TODO: pass in format 1h30m15s
		[Description("The reason of the mute")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;
		
		DateTimeOffset TimeoutTime = DateTimeOffset.UtcNow.AddMinutes(Duration);
		await Member.TimeoutAsync(TimeoutTime, Reason);
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)}",
			Description = 
				$"Member {Member.DisplayName} was muted for {Duration} minute{(Duration == 1 ? "" : "s")} by {Context.Member.DisplayName} {CEmoji.GirBlep}" +
				$"\n\n Reason: {Reason}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}
	
	[Command("unmute")]
	[Description("Unmutes the specified member")]
	public async Task Unmute(CommandContext Context, 
		[Description("The member to unmute")] DiscordMember Member)
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}
		
		if (IsTargetingBotOrSelf(Context, Member).Result) return;

		if (!Member.CommunicationDisabledUntil.HasValue || Member.CommunicationDisabledUntil.Value <= DateTimeOffset.UtcNow)
		{
			await Context.RespondAsync($"{Member.DisplayName} is not currently timed out {CEmoji.ZimAngry}");
			return;
		}
		
		await Member.TimeoutAsync(null); // TODO: reason
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = $"Member {Member.DisplayName} was unmuted by {Context.Member.DisplayName}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}

	[Command("purge")]
	[Description("Purges specified messages count in a channel")]
	public async Task Purge(CommandContext Context,
		[Description("Count of the messages to purge")] Int32 MessageCount,
		[Description("The channel to purge messages in")] DiscordChannel? Channel = null)
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		bool bContextChannel = IsContextChannel(Context, ref Channel);
		if (bContextChannel)
		{
			// Include a message that triggered the execution
			MessageCount++;
		}
		
		if (MessageCount is 0 or > 100)
		{
			if (bContextChannel)
			{
				await Context.Message.DeleteAsync();
			}
			else
			{
				await Context.RespondAsync($"Provide a valid message count, Earth pig! {CEmoji.ZimAngry}");
			}
			return;
		}

		IReadOnlyList<DiscordMessage>? Messages = await Channel.GetMessagesAsync(MessageCount);
		List<DiscordMessage> MessagesToDelete = bContextChannel ? Messages.ToList() : Messages.Where(m => m.Id != Context.Message.Id).ToList();
		
		if (MessagesToDelete.Count != 0)
		{
			// TODO: handle messages older than 14 days
			Debug.Assert(Context.Member != null);
			await Channel.DeleteMessagesAsync(MessagesToDelete, $"Purge command executed by {Context.Member.DisplayName}");

			DiscordMessage Response = await Context.RespondAsync($"Successfully deleted {MessagesToDelete.Count} messages in {Channel.Mention} channel {CEmoji.GirBlep}");
			if (bContextChannel)
			{
				await Task.Delay(TimeSpan.FromSeconds(TemporaryResponseTime));
				await Response.DeleteAsync();
			}
		}
		else
		{
			// TODO: Response
		}
	}

	[Command("prune")]
	[Description("Prunes messages up to a specific age in a channel (max 100 per request)")]
	public async Task Prune(CommandContext Context,
		[Description("Age of messages to purge (e.g., 1h30m, 10m5s)")] string Time,
		[Description("The channel to prune messages in")] DiscordChannel? Channel = null)
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		bool bContextChannel = IsContextChannel(Context, ref Channel);

		DateTimeOffset CutOff = DateTimeOffset.UtcNow.Subtract(ParseTime(Time));
		DateTimeOffset FourteenDaysAgo = DateTimeOffset.UtcNow.AddDays(-14);
		if (CutOff < FourteenDaysAgo)
		{
			DiscordMessage Response = await Context.RespondAsync($"I cannot delete messages older than 14 days due to Discord restrictions {CEmoji.GirBlep}");
			if (bContextChannel)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
				await Response.DeleteAsync();
				await Context.Message.DeleteAsync();
				return;
			}
		}

		IReadOnlyList<DiscordMessage>? Messages = await Channel.GetMessagesAsync(100);
		List<DiscordMessage> MessagesToDelete = Messages
			.Where(m => m.Id != Context.Message.Id)
			.Where(m => m.CreationTimestamp >= CutOff).ToList();

		if (MessagesToDelete.Count != 0)
		{
			Debug.Assert(Context.Member != null);
			await Channel.DeleteMessagesAsync(MessagesToDelete, $"Prune command executed by {Context.Member.DisplayName}");

			DiscordMessage Response = await Context.RespondAsync($"Successfully deleted {MessagesToDelete.Count} messages in {Channel.Mention} channel {CEmoji.GirBlep}");
			if (bContextChannel)
			{
				await Task.Delay(TimeSpan.FromSeconds(TemporaryResponseTime));
				await Response.DeleteAsync();
			}
		}

		if (bContextChannel)
		{
			await Context.Message.DeleteAsync();
		}
	}

	[Command("reap")]
	[Description("Grim-reaps the specified member. Automatically issues a ban after reaching 3 warnings")]
	public async Task Warn(CommandContext Context,
		[Description("The member to grim-reap")] DiscordMember Member,
		[Description("The reason of the grim-reap")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;

		DiscordRole WarnedOnceRole = Context.Guild.GetRole(CRole.WarnedOnce);
		DiscordRole WarnedTwiceRole = Context.Guild.GetRole(CRole.WarnedTwice);

		Int32 WarnsCount;
		if (Member.Roles.Contains(WarnedOnceRole))
		{
			await Member.RevokeRoleAsync(WarnedOnceRole);
			await Member.GrantRoleAsync(WarnedTwiceRole);

			WarnsCount = 2;
		}
		else if (Member.Roles.Contains(WarnedTwiceRole))
		{
			await Ban(Context, Member, $"Got third warning. {Reason}");

			WarnsCount = 3;
		}
		else
		{
			await Member.GrantRoleAsync(WarnedOnceRole);

			WarnsCount = 1;
		}

		string PunishmentSummary = WarnsCount switch
		{
			1 => "was warned first time",
			_ => "was warned second time"
		};

		if (WarnsCount != 3) // Response is handled in Ban() method
		{
			Debug.Assert(Context.Member != null);
			DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
			{
				Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
				Description =
					$"Member {Member.DisplayName} {PunishmentSummary} by {Context.Member.DisplayName}" +
					$"\n\n Reason: {Reason}",
				Color = YellowGreen
			};
			await Context.RespondAsync(Embed);
		}
	}

	[Command("unwarn")]
	[Description("Removes a warning from the member")]
	[RequirePermissions(Permissions.Administrator)]
	public async Task Unwarn(CommandContext Context, 
		[Description("The member to remove warning from")] DiscordMember Member)
	{
		// TODO: we already have RequirePermissions(Permissions.Administrator)
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;

		DiscordRole WarnedOnceRole = Context.Guild.GetRole(CRole.WarnedOnce);
		DiscordRole WarnedTwiceRole = Context.Guild.GetRole(CRole.WarnedTwice);

		Int32 WarnsCount;
		if (Member.Roles.Contains(WarnedOnceRole))
		{
			await Member.RevokeRoleAsync(WarnedOnceRole);
		}
		else if (Member.Roles.Contains(WarnedTwiceRole))
		{
			await Member.RevokeRoleAsync(WarnedTwiceRole);
			await Member.GrantRoleAsync(WarnedOnceRole);
		}
		else
		{
			// TODO: "User does not have any warnings!"
		}

		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = 
				$"Member {Member.DisplayName} got unwarned by {Context.Member.DisplayName}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}
	
	[Command("kick")]
	[Description("Kicks the specified member")]
	public async Task Kick(CommandContext Context, 
		[Description("The member to kick")] DiscordMember Member,
		[Description("The reason of the kick")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;
		
		await Member.RemoveAsync(Reason);
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = 
				$"Member {Member.DisplayName} was kicked by {Context.Member.DisplayName}" +
				$"\n\n Reason: {Reason}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}
	
	[Command("ban")]
	[Description("Bans the specified member")]
	public async Task Ban(CommandContext Context, 
		[Description("The member to ban")] DiscordMember Member,
		[Description("The reason of the ban")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;

		await Context.Guild.BanMemberAsync(Member);
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = 
				$"Member {Member.DisplayName} was banned by {Context.Member.DisplayName}" +
				$"\n\n Reason: {Reason}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}
	
	[Command("unban")]
	[Description("Removes a ban from specified member")]
	public async Task UnBan(CommandContext Context, 
		[Description("The member to unban")] DiscordUser User)
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		// TODO: can't pass DiscordUser here
		// if (IsTargetingBotOrSelf(Context, User).Result) return;
		
		DiscordBan? Ban = await Context.Guild.GetBanAsync(User);
		if (Ban == null)
		{
			await Context.RespondAsync($"{User.Username} is not currently banned on the server {CEmoji.ZimAngry}");
			return;
		}
			
		await Context.Guild.UnbanMemberAsync(User);
		await Context.RespondAsync($"Successfully unbanned {User.Username}");
	}
}
