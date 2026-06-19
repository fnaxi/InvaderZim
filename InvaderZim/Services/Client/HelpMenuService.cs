// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;

namespace InvaderZim.Services.Client;

public class CHelpMenuService
{
	public CHelpMenuService(DiscordClient Client)
	{
		Client.ComponentInteractionCreated += Client_OnComponentInteractionCreated;
	}

	private const string HelpTitlePrefix = $"{CEmoji.GirDance} Gir |";

	public static DiscordEmbedBuilder MakeMainMenuEmbed(DiscordMember Member)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{HelpTitlePrefix} Help Menu {CEmoji.BmoDance}",
			Description = 
				$"Welcome {Member.Mention}!\n" + 
				$"Use the dropdown menu below to navigate through command categories {CEmoji.GirBlep}",
			Color = YellowGreen
		};
		Embed.WithFooter($"Requested by {Member.DisplayName}", Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);
		return Embed;
	}

	private async Task Client_OnComponentInteractionCreated(DiscordClient Sender, ComponentInteractionCreateEventArgs Args)
	{
		if (!Args.Id.StartsWith("SID_HelpMenu_")) return;
		
		string OwnerIdText = Args.Id.Replace("SID_HelpMenu_", "");
		if (!UInt64.TryParse(OwnerIdText, out UInt64 OwnerId))
		{
			LogError($"Couldn't parse an owner ID from SID_HelpMenu event: {OwnerIdText}");
			return;
		}

		DiscordMember Owner = await Args.Guild.GetMemberAsync(OwnerId);
		if (Args.User.Id != OwnerId)
		{
			DiscordInteractionResponseBuilder Response = new DiscordInteractionResponseBuilder();
			Response.WithContent($":x: This help menu belongs to {Owner.DisplayName}! Use `help` to get one.");
			
			await Args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, Response);
			return;
		}

		DiscordEmbed Embed = Args.Message.Embeds[0];

		string SelectedOption = Args.Values[0];
		DiscordEmbedBuilder UpdatedEmbed = SelectedOption switch
		{
			"SID_Choose" => new DiscordEmbedBuilder(Embed),
			// TODO: "main" => GetMainMenuEmbed(Owner),
			
			"SID_Mod" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Moderation Commands :shield:")
				.WithDescription(
					"**NOTE:** These commands can only be executed by moderators.\n\n" +
					"• `mute <user> ?<reason>` - Mutes (timeouts) a member.\n" +
					"• `unmute <user>` - Removes a timeout from a member.\n" +
					"• `reap <user> ?<reason>` - Grim-reaps (warns) the specified member. Automatically issues a ban after reaching 3 warnings.\n" +
					"• `unwarn <user>` - Removes a warning from a member (can only be used by administrators).\n" +
					"• `kick <user> ?<reason>` - Kicks a member.\n" +
					"• `ban <user> ?<reason>` - Permanently bans the specified member.\n" +
					"• `unban <user> ?<reason>` - Removes a ban from specified member.\n" +
					"• `purge <amount>` - Deletes specific amount of messages (up to 99).\n" +
					"• `prune <time>` - Deletes messages older than a specified time (up to 14 days due to Discord restrictions) (in format 2h15m, 1d6h, etc).")
				.WithColor(DiscordColor.Azure),

			"SID_Misc" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Misc & Utilities :gear:")
				.WithDescription(
					$"• `help` - Shows help menu (like this one haha {CEmoji.GirDance}).\n" +
					"• `info` - Shows information about a member.\n" +
					"• `ping` - Checks bot latency.\n" +
					"• `shutdown` - Shutdowns the bot.")
				.WithColor(DiscordColor.Gray),

			"SID_Entertain" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Entertainment Commands :tada:")
				.WithDescription(
					"• `feed waffles` - Gives gir some tasty waffles :waffle: to eat.\n"+ 
				    "• `feed tacos` - Gives gir some tacos :taco: to eat.")
				.WithColor(DiscordColor.Yellow),
			
			"SID_Ticket" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Ticket Commands :ticket:")
				.WithDescription(
					"**NOTE:** These commands should be executed in the ticket channel.\n\n" +
					"• `ticket close` - Closes this ticket.\n" +
					"• `ticket open` - Re-opens this ticket (can only be used by moderators).\n" +
					"• `ticket delete` - Deletes this ticket (can only be used by administrators).")
				.WithColor(DiscordColor.Teal),
			
			"SID_Manage" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Management Commands :tools:")
				.WithDescription(
					$"**NOTE:** These commands can only be executed by administrators {CEmoji.BmoDance}\n\n" +
					$"• `news` - Reposts a message user replied to in the {Args.Guild.GetChannel(CChannel.News).Mention} channel.\n" +
					"• `send rules` - Sends rules message.\n" +
					"• `send color_roles` - Sends color roles message.\n" +
					"• `send ticket_form` - Sends ticket form message.")
				.WithColor(DiscordColor.Brown),
			
			"SID_Test" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Test Commands :warning:")
				.WithDescription(
					$"**NOTE:** These commands can only be executed by administrators {CEmoji.BmoDance}\n\n" +
					"• `emojis` - Sends an embed with some custom animated emojis.")
				.WithColor(DiscordColor.Orange),

			_ => new DiscordEmbedBuilder(Embed).WithDescription("Unknown category.")
		};
		
		DiscordInteractionResponseBuilder InteractionResponse = new DiscordInteractionResponseBuilder();
		InteractionResponse.AddEmbed(UpdatedEmbed);
		InteractionResponse.AddComponents(Args.Message.Components.Cast<DiscordActionRowComponent>()
			.SelectMany<DiscordActionRowComponent, DiscordComponent>(row => row.Components));
		
		await Args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, InteractionResponse);
	}
}