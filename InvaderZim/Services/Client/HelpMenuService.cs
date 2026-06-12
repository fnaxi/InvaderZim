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

	private const string HelpTitlePrefix = "Gir |";

	public static DiscordEmbedBuilder GetMainMenuEmbed(DiscordMember Member)
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
			"choose" => new DiscordEmbedBuilder(Embed),
			"main" => GetMainMenuEmbed(Owner),
			
			"mod" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Moderation Commands :shield:")
				.WithDescription(
					"• `mute <user> ?<reason>` - Mutes (timeouts) a member.\n" +
					"• `unmute <user>` - Removes a timeout from a member (can only be used by administrators).\n" +
					"• `kick <user> ?<reason>` - Kicks a member.\n" +
					"• `ban <user> ?<reason>` - Bans a member.\n" +
					"• `purge <amount>` - Deletes specific amount of messages (up to 99).\n" +
					"• `prune <time>` - Deletes messages older than a specified time (up to 14 days due to Discord restrictions) (in format 2h15m, 1d6h, etc).")
				.WithColor(DiscordColor.Azure),

			"misc" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Misc & Utilities :gear:")
				.WithDescription(
					"• `info` - Shows information about a member.\n" +
					"• `ping` - Checks bot latency.\n" +
					"• `shutdown` - Shutdowns the bot.")
				.WithColor(DiscordColor.Gray),

			"entertain" => new DiscordEmbedBuilder(Embed)
				.WithTitle($"{HelpTitlePrefix} Entertainment Commands :tada:")
				.WithDescription(
					"• `waffles` - Gives gir some tasty waffles to eat.\n"+ 
				    "• `tacos` - Gives gir some tacos to eat.")
				.WithColor(DiscordColor.Gold),
			
			// TODO: Management commands

			_ => new DiscordEmbedBuilder(Embed).WithDescription("Unknown category.")
		};
		
		DiscordInteractionResponseBuilder InteractionResponse = new DiscordInteractionResponseBuilder();
		InteractionResponse.AddEmbed(UpdatedEmbed);
		InteractionResponse.AddComponents(Args.Message.Components.Cast<DiscordActionRowComponent>()
			.SelectMany<DiscordActionRowComponent, DiscordComponent>(row => row.Components));
		
		await Args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, InteractionResponse);
	}
}