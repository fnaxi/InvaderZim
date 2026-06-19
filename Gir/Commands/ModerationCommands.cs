// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Gir.ID;
using Gir.Misc;

namespace Gir.Commands;

public static class CModeration
{
	public class CWarnSummary(Int32 InWarnsCount, string InPunishmentSummary)
	{
		public readonly Int32 WarnsCount = InWarnsCount;
		public readonly string PunishmentSummary = InPunishmentSummary;
		public static CWarnSummary Default = new(0, "No reason provided.");
	}
	public static async Task<CWarnSummary> Warn(DiscordGuild Guild, DiscordMember Moderator, DiscordMember Member, DiscordMessage Message, string Reason)
	{
		DiscordRole WarnedOnceRole = Guild.GetRole(CRole.WarnedOnce);
		DiscordRole WarnedTwiceRole = Guild.GetRole(CRole.WarnedTwice);

		Int32 WarnsCount;
		if (Member.Roles.Contains(WarnedOnceRole))
		{
			await Member.RevokeRoleAsync(WarnedOnceRole);
			await Member.GrantRoleAsync(WarnedTwiceRole);

			WarnsCount = 2;
		}
		else if (Member.Roles.Contains(WarnedTwiceRole))
		{
			await Ban(Guild, Moderator, Member, Message, $"Got third warning. {Reason}");

			WarnsCount = 3;
		}
		else
		{
			await Member.GrantRoleAsync(WarnedOnceRole);

			WarnsCount = 1;
		}

		string PunishmentSummary = WarnsCount switch
		{
			1 => "was grim-reaped first time",
			_ => "was grim-reaped second time"
		};

		Debug.Assert(Moderator != null);
		if (WarnsCount != 3) // Response is handled in Ban() method
		{
			DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
			{
				Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
				Description =
					$"Member {Member.DisplayName} {PunishmentSummary} by {Moderator.DisplayName}" +
					$"\n\n Reason: {Reason}",
				Color = YellowGreen
			};
			await Message.RespondAsync(Embed);
		}

		return new CWarnSummary(WarnsCount, PunishmentSummary);
	}

	public static async Task Ban(DiscordGuild Guild, DiscordMember Moderator, DiscordMember Member, DiscordMessage Message, string Reason)
	{
		await Guild.BanMemberAsync(Member);
		
		Debug.Assert(Moderator != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = 
				$"Member {Member.DisplayName} was banned by {Moderator.DisplayName}" +
				$"\n\n Reason: {Reason}",
			Color = YellowGreen
		};
		await Message.RespondAsync(Embed);
	}
}

public class CModerationCommands : BaseCommandModule
{
	// TODO: log command (audit log) (only mute/unmute/kick/ban/delete_msg)
	
	[Command("mute")]
	[Description("Mutes the specified member")]
	public async Task Mute(CommandContext Context, 
		[Description("The member to mute")] DiscordMember Member,
		[Description("Time of the mute up to 28 days (e.g., 1h30m, 10m5s)")] string Time,
		[Description("The reason of the mute")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		if (await IsTargetingBotOrSelf(Context, Member)) return;
		
		DateTimeOffset TimeoutTime = DateTimeOffset.UtcNow.Subtract(ParseTime(Time));
		if ( (DateTimeOffset.UtcNow - TimeoutTime).TotalDays > 28 )
		{
			await Context.RespondAsync("Mute time cannot be longer than 28 days!");
			return;
		}
		
		await Member.TimeoutAsync(TimeoutTime, Reason);
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)}",
			Description = 
				$"Member {Member.DisplayName} was muted for {Time} by {Context.Member.DisplayName} {CEmoji.GirBlep}" +
				$"\n\n Reason: {Reason}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
		
		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{Member.DisplayName} was muted for {Time} {CEmoji.GirBlep}",
			Description = 
				$"\n\n ID: {Member.Id}" +
				$"\nReason: {Reason}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(Member.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);

		await SendToModLog(Context, ModLogEmbed);
	}
	
	[Command("unmute")]
	[Description("Unmutes the specified member")]
	public async Task Unmute(CommandContext Context, 
		[Description("The member to unmute")] DiscordMember Member)
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}
		
		if (await IsTargetingBotOrSelf(Context, Member)) return;

		if (!Member.CommunicationDisabledUntil.HasValue || Member.CommunicationDisabledUntil.Value <= DateTimeOffset.UtcNow)
		{
			await Context.RespondAsync($"{Member.DisplayName} is not currently timed out {CEmoji.ZimAngry}");
			return;
		}
		
		await Member.TimeoutAsync(null);
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{RandomString(CQuote.Ban)} {CEmoji.GirBlep}",
			Description = $"Member {Member.DisplayName} was unmuted by {Context.Member.DisplayName}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
		
		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{Member.DisplayName} was unmuted {CEmoji.GirBlep}",
			Description = 
				$"\n\n ID: {Member.Id}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(Member.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);

		await SendToModLog(Context, ModLogEmbed);
	}
	
	// TODO: review Administrator permission for Purge and Prune commands

	[Command("purge")]
	[Description("Purges specified messages count in a channel")]
	[RequirePermissions(Permissions.Administrator)]
	public async Task Purge(CommandContext Context,
		[Description("Count of the messages (up to 99) to purge")] Int32 MessageCount,
		[Description("The channel to purge messages in")] DiscordChannel? Channel = null)
	{
		if (!CanModerate(Context.Member))
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
		Debug.Assert(Channel != null);
		
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
			await Context.RespondAsync($"There are no messages to delete, Earth pig! {CEmoji.ZimAngry}");
		}
	}

	[Command("prune")]
	[Description("Prunes messages up to a specific age in a channel (max 100 per request)")]
	[RequirePermissions(Permissions.Administrator)]
	public async Task Prune(CommandContext Context,
		[Description("Age of messages (up to 14 days) to purge (e.g., 1h30m, 10m5s)")] string Time,
		[Description("The channel to prune messages in")] DiscordChannel? Channel = null)
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		bool bContextChannel = IsContextChannel(Context, ref Channel);
		Debug.Assert(Channel != null);

		DateTimeOffset CutOff = DateTimeOffset.UtcNow.Subtract(ParseTime(Time));
		DateTimeOffset FourteenDaysAgo = DateTimeOffset.UtcNow.AddDays(-14);
		if (CutOff < FourteenDaysAgo)
		{
			DiscordMessage Response = await Context.RespondAsync($"I cannot delete messages older than 14 days due to Discord restrictions {CEmoji.ZimAngry}");
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
		else
		{
			await Context.RespondAsync($"There are no messages to delete, Earth pig! {CEmoji.ZimAngry}");
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
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		if (await IsTargetingBotOrSelf(Context, Member)) return;

		CModeration.CWarnSummary Summary = await CModeration.Warn(Context.Guild, Context.Member, Member, Context.Message, Reason);
		if (Summary.WarnsCount != 3)
		{
			DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
			{
				Title = $"{Member.DisplayName} {Summary.PunishmentSummary} {CEmoji.GirBlep}",
				Description =
					$"\n\nID: {Member.Id}" +
					$"\nReason: {Reason}",
				Color = YellowGreen
			};
			ModLogEmbed.WithThumbnail(Member.AvatarUrl);
			ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
			ModLogEmbed.WithTimestamp(DateTime.UtcNow);

			await SendToModLog(Context, ModLogEmbed);
		}
	}

	[Command("unwarn")]
	[Description("Removes a warning from the member")]
	[RequirePermissions(Permissions.Administrator)]
	public async Task Unwarn(CommandContext Context, 
		[Description("The member to remove warning from")] DiscordMember Member)
	{
		// TODO: we already have RequirePermissions(Permissions.Administrator)
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		if (await IsTargetingBotOrSelf(Context, Member)) return;

		DiscordRole WarnedOnceRole = Context.Guild.GetRole(CRole.WarnedOnce);
		DiscordRole WarnedTwiceRole = Context.Guild.GetRole(CRole.WarnedTwice);

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
				$"Member {Member.DisplayName} was unwarned by {Context.Member.DisplayName}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
		
		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{Member.DisplayName} was unwarned {CEmoji.GirBlep}",
			Description =
				$"\n\nID: {Member.Id}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(Member.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);

		await SendToModLog(Context, ModLogEmbed);
	}
	
	[Command("kick")]
	[Description("Kicks the specified member")]
	public async Task Kick(CommandContext Context, 
		[Description("The member to kick")] DiscordMember Member,
		[Description("The reason of the kick")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		if (await IsTargetingBotOrSelf(Context, Member)) return;
		
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
		
		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{Member.DisplayName} was kicked {CEmoji.GirBlep}",
			Description =
				$"\n\nID: {Member.Id}" +
				$"\nReason: {Reason}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(Member.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);

		await SendToModLog(Context, ModLogEmbed);
	}
	
	// TODO: add an option to delete all messages the member sent
	[Command("ban")]
	[Description("Permanently bans the specified member")]
	public async Task Ban(CommandContext Context, 
		[Description("The member to ban")] DiscordMember Member,
		[Description("The reason of the ban")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		if (await IsTargetingBotOrSelf(Context, Member)) return;

		await CModeration.Ban(Context.Guild, Context.Member, Member, Context.Message, Reason);

		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{Member.DisplayName} was banned {CEmoji.GirBlep}",
			Description =
				$"\n\nID: {Member.Id}" +
				$"\nReason: {Reason}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(Member.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);
		await SendToModLog(Context, ModLogEmbed);
	}
	
	[Command("unban")]
	[Description("Removes a ban from specified member")]
	public async Task UnBan(CommandContext Context, 
		[Description("The user to unban")] DiscordUser User)
	{
		if (!CanModerate(Context.Member))
		{
			await NoRights(Context);
			return;
		}

		// TODO: can't pass DiscordUser here
		// if (await IsTargetingBotOrSelf(Context, User)) return;
		
		DiscordBan? Ban = await Context.Guild.GetBanAsync(User);
		if (Ban == null)
		{
			await Context.RespondAsync($"{User.Username} is not currently banned on the server {CEmoji.ZimAngry}");
			return;
		}
			
		await Context.Guild.UnbanMemberAsync(User);
		await Context.RespondAsync($"Successfully unbanned {User.Username}");
		
		Debug.Assert(Context.Member != null);
		DiscordEmbedBuilder ModLogEmbed = new DiscordEmbedBuilder()
		{
			Title = $"{User.Username} was unbanned {CEmoji.GirBlep}",
			Description =
				$"\n\nID: {User.Id}",
			Color = YellowGreen
		};
		ModLogEmbed.WithThumbnail(User.AvatarUrl);
		ModLogEmbed.WithFooter($"Moderator: {Context.Member.DisplayName}", Context.Member.AvatarUrl);
		ModLogEmbed.WithTimestamp(DateTime.UtcNow);

		await SendToModLog(Context, ModLogEmbed);
	}
	
	private async Task SendToModLog(CommandContext Context, DiscordEmbed Embed)
	{
		DiscordChannel ModLogChannel = Context.Guild.GetChannel(CChannel.ModLog);
		
		await ModLogChannel.SendMessageAsync(Embed);
	}
}
