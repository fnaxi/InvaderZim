// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;

namespace InvaderZim.Services.Client;

public class CTicket
{
	public static readonly string Prefix = "メticket-";
	public static readonly string ClosedSuffix = "-closed";
	
	public CTicket(string InName, Int32 InId, UInt64 InChannelId, UInt64 InGuildId, bool bInClosed)
	{
		Name = InName;
		Id = InId;
		ChannelId = InChannelId;
		GuildId = InGuildId;
		bClosed = bInClosed;
	}
	
	public string Name;
	public Int32 Id;
	public UInt64 ChannelId;
	public UInt64 GuildId;
	public bool bClosed;

	public async Task Open(DiscordGuild Guild, DiscordUser OpenedByUser)
	{
		if (!bClosed) return;

		DiscordChannel Channel = Guild.GetChannel(ChannelId);
		
		await Channel.ModifyAsync(a => a.Name = Name[..^ClosedSuffix.Length]);
		await Channel.SendMessageAsync($"The ticket was re-opened by {OpenedByUser.Mention} ({OpenedByUser.Id})");
		
		bClosed = false;
	}
	
	public async Task Close(DiscordGuild Guild, DiscordUser ClosedByUser, string Reason = "No reason provided")
	{
		if (bClosed) return;

		DiscordChannel Channel = Guild.GetChannel(ChannelId);
		
		await Channel.ModifyAsync(a => a.Name = $"{Name}{ClosedSuffix}");
		await Channel.SendMessageAsync($"The ticket was closed by {ClosedByUser.Mention} ({ClosedByUser.Id})\n\nReason: {Reason}");
		
		bClosed = true;
	}
}

public class CTicketsService
{
	public CTicketsService(DiscordClient Client)
	{
		Client.GuildDownloadCompleted += Client_OnGuildDownloadCompleted;
		Client.ComponentInteractionCreated += Client_OnComponentInteractionCreated;
	}

	public readonly List<CTicket> Tickets = new();

	private async Task Client_OnGuildDownloadCompleted(DiscordClient Sender, GuildDownloadCompletedEventArgs Args)
	{
		foreach (DiscordGuild Guild in Args.Guilds.Values)
		{
			LogDebug($"Guild {Guild.Name} ({Guild.Id})");
			IReadOnlyList<DiscordChannel> Channels = await Guild.GetChannelsAsync();
			foreach (DiscordChannel Channel in Channels)
			{
				if (!IsTicketChannel(Channel) || Channel.Name.EndsWith(CTicket.Prefix)) continue;

				// TODO: check there are no tickets with same ID
				
				bool bClosed = Channel.Name.Contains(CTicket.ClosedSuffix);
				Tickets.Add(new CTicket(Channel.Name, Int32.Parse(Channel.Name.Split('-')[1]), Channel.Id, Guild.Id, bClosed));
			}
		}
	}

	private async Task Client_OnComponentInteractionCreated(DiscordClient Sender, ComponentInteractionCreateEventArgs Args)
	{
		switch (Args.Id)
		{
			case "SID_CreateTicket":
			{
				Int32 Id = Tickets.Count + 1;
				string Name = $"{CTicket.Prefix}{(Id):D4}";
				DiscordChannel ModerationCategory = Args.Guild.GetChannel(CCategory.Moderation);

				List<DiscordOverwriteBuilder> Permissions =
				[
					new DiscordOverwriteBuilder(Args.Guild.EveryoneRole).Deny(DSharpPlus.Permissions.AccessChannels),
					new DiscordOverwriteBuilder(Args.User as DiscordMember).Allow(DSharpPlus.Permissions.AccessChannels),
					new DiscordOverwriteBuilder(Args.Guild.GetRole(CRole.Moderator)).Allow(DSharpPlus.Permissions.AccessChannels)
				];

				DiscordChannel Channel = await Args.Guild.CreateChannelAsync(Name, ChannelType.Text, ModerationCategory, default, null, null, Permissions);

				DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
				{
					Title = $"{Name} {CEmoji.GirDance}",
					Description =
						"\n" +
						$"Support will be with you shortly!\n" +
						$"Click the button below to close this ticket.\n" +
						$"\n" +
						$"Author: {Args.User.Username} / {Args.User.Id}\n",
					Color = YellowGreen
				};

				DiscordComponentEmoji Emoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Sender, ":lock:"));
				DiscordButtonComponent CloseButton = new DiscordButtonComponent(ButtonStyle.Primary, "SID_CloseTicket", "Close", false, Emoji);

				DiscordMessageBuilder MessageEmbed = new DiscordMessageBuilder();
				MessageEmbed.WithContent($"{Args.User.Mention} Welcome!");
				MessageEmbed.WithEmbed(Embed);
				MessageEmbed.AddComponents(CloseButton);

				DiscordMessage Message = await Channel.SendMessageAsync(MessageEmbed);
				await Message.PinAsync();
				
				// TODO: split this case into methods

				Tickets.Add(new CTicket(Name, Id, Channel.Id, Args.Guild.Id, false));
				LogInfo($"Created new ticket ({Name}/{Id}, Channel/{Channel.Id}, Guild/{Args.Guild.Id})");

				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
			case "SID_CloseTicket":
			{
				CTicket Ticket = Tickets.Find(t => t.Name == Args.Channel.Name);
				Debug.Assert(Ticket != null);
				
				await Ticket.Close(Args.Guild, Args.User);
				
				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
		}
	}

	public async Task DeleteTicket(DiscordGuild Guild, CTicket Ticket)
	{
		DiscordChannel Channel = Guild.GetChannel(Ticket.ChannelId);
		await Channel.DeleteAsync();
		
		Tickets.Remove(Ticket);
	}
	
	private List<CTicket> GetTicketsFromGuild(DiscordGuild Guild)
	{
		return Tickets.Where(t => t.GuildId == Guild.Id).ToList();
	}

	public static bool IsTicketChannel(DiscordChannel Channel)
	{
		return Channel.Type == ChannelType.Text && Channel.Name.Contains(CTicket.Prefix) && Channel.Id != CChannel.TicketCreator;
	}
}
