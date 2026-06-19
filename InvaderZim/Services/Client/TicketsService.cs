// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;

namespace InvaderZim.Services.Client;

public class CTicket(string InName, Int32 InId, UInt64 InChannelId, UInt64 InGuildId, bool bInClosed)
{
	public readonly string Name = InName;
	public readonly Int32 Id = InId;
	public readonly UInt64 ChannelId = InChannelId;
	public readonly UInt64 GuildId = InGuildId;
	public bool bClosed = bInClosed;

	public static readonly string Prefix = "メticket-";
	public static readonly string ClosedSuffix = "-closed";
	
	public async Task Open(DiscordGuild Guild, DiscordUser OpenedByUser)
	{
		if (!bClosed) return;
		bClosed = false;
		
		DiscordChannel Channel = Guild.GetChannel(ChannelId);
		
		await Channel.ModifyAsync(a => a.Name = Channel.Name.Substring(0, Channel.Name.Length - ClosedSuffix.Length));
		await Task.Delay(TimeSpan.FromSeconds(0.5));
		await Channel.SendMessageAsync($"The ticket was re-opened by {OpenedByUser.Mention} ({OpenedByUser.Id})");
	}
	
	public async Task Close(DiscordGuild Guild, DiscordUser ClosedByUser, string Reason = "No reason provided")
	{
		if (bClosed) return;
		bClosed = true;
		
		DiscordChannel Channel = Guild.GetChannel(ChannelId);
		
		await Channel.ModifyAsync(a => a.Name = $"{Name}{ClosedSuffix}");
		await Task.Delay(TimeSpan.FromSeconds(0.5));
		await Channel.SendMessageAsync($"The ticket was closed by {ClosedByUser.Mention} ({ClosedByUser.Id})\n\nReason: {Reason}");
	}
}

public class CTicketsService
{
	public CTicketsService(DiscordClient Client)
	{
		Client.GuildDownloadCompleted += Client_OnGuildDownloadCompleted;
		Client.ComponentInteractionCreated += Client_OnComponentInteractionCreated;
	}

	public static readonly List<CTicket> Tickets = [];

	private async Task Client_OnGuildDownloadCompleted(DiscordClient Sender, GuildDownloadCompletedEventArgs Args)
	{
		foreach (DiscordGuild Guild in Args.Guilds.Values)
		{
			LogInfo($"Collecting tickets for {Guild.Name} ({Guild.Id}) guild");
			IReadOnlyList<DiscordChannel> Channels = await Guild.GetChannelsAsync();
			foreach (DiscordChannel Channel in Channels)
			{
				if (!IsTicketChannel(Channel)) continue;

				Int32 ID = Int32.Parse(Channel.Name.Split('-')[1]);
				if (Tickets.Any(t => t.Id == ID))
				{
					CTicket? OriginalTicket = Tickets.Find(t => t.Id == ID);
					
					LogError($"Found two tickets with the same ID: {OriginalTicket.Name} / {Channel.Name}");
					continue;
				}
				
				bool bClosed = Channel.Name.Contains(CTicket.ClosedSuffix);
				Tickets.Add(new CTicket(Channel.Name, ID, Channel.Id, Guild.Id, bClosed));
			}
			foreach (CTicket Ticket in Tickets)
			{
				LogInfo($"{Guild.Name} ({Guild.Id}): Found ticket {Ticket.Name} (ID/{Ticket.Id}, Channel/{Ticket.ChannelId}, Closed?/{Ticket.bClosed})");
			}
		}
	}

	private async Task Client_OnComponentInteractionCreated(DiscordClient Sender, ComponentInteractionCreateEventArgs Args)
	{
		switch (Args.Id)
		{
			case "SID_CreateTicket":
			{
				// TODO: split this case into methods
				
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

	public static async Task DeleteTicket(DiscordGuild Guild, CTicket Ticket)
	{
		DiscordChannel Channel = Guild.GetChannel(Ticket.ChannelId);
		await Channel.DeleteAsync();
		
		Tickets.Remove(Ticket);
	}

	public static bool IsTicketChannel(DiscordChannel Channel)
	{
		return Channel.Type == ChannelType.Text && Channel.Name.Contains(CTicket.Prefix) && Channel.Id != CChannel.TicketCreator;
	}
}
