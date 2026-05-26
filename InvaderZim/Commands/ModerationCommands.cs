// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim.Commands;

public class CModerationCommands : BaseCommandModule
{
	[Command("mute")]
	[Description("Mutes the specified member")]
	public async Task Mute(CommandContext Context, 
		[Description("The member to mute")] DiscordMember Member,
		[Description("Time of the mute in minutes")] UInt32 Duration = 30,
		[Description("The reason of the mute")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;
		
		// TODO: replace with format 1d6h2m5s
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
				await Task.Delay(TimeSpan.FromSeconds(3));
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
				await Task.Delay(TimeSpan.FromSeconds(3));
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
				await Task.Delay(TimeSpan.FromSeconds(3));
				await Response.DeleteAsync();
			}
		}

		// TODO: revisit this and if (CutOff < FourteenDaysAgo) condition above
		if (bContextChannel)
		{
			await Context.Message.DeleteAsync();
		}
	}

	// TODO: reap(member, reason)
	
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
		
		// TODO: Actual kick
		// await Member.RemoveAsync(Reason);
		
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
		
		// TODO: Actual ban
		// await Context.Guild.BanMemberAsync(Member);
		
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
		[Description("The member to unban")] DiscordMember Member)
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}

		if (IsTargetingBotOrSelf(Context, Member).Result) return;
		
		// TODO: unban command
	}
}
