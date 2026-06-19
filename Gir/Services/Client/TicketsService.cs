// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gir.ID;

namespace Gir.Services.Client;

public class CTicket(string InName, Int32 InId, UInt64 InChannelId, UInt64 InGuildId, bool bInClosed)
{
	public readonly string Name = InName;
	public readonly Int32 Id = InId;
	public readonly UInt64 ChannelId = InChannelId;
	public readonly UInt64 GuildId = InGuildId;
	public bool bClosed = bInClosed;

	public static readonly string Prefix = "メticket-";
	public static readonly string ClosedSuffix = "-closed";

	public async Task Open(DiscordGuild Guild, DiscordUser Moderator, DiscordMessage? Message = null)
	{
		if (!bClosed) return;
		bClosed = false;

		DiscordChannel Channel = Guild.GetChannel(ChannelId);

		await Channel.ModifyAsync(a => a.Name = Channel.Name.Substring(0, Channel.Name.Length - ClosedSuffix.Length));
		await Task.Delay(TimeSpan.FromSeconds(1.5));
		
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"The ticket was re-opened {CEmoji.GirBlep}",
			Description = $"",
			Color = YellowGreen
		};
		Embed.WithFooter($"Moderator: {Moderator.Username}", Moderator.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);
		
		if (Message != null)
		{
			await Message.RespondAsync(Embed);
		}
		else
		{
			await Channel.SendMessageAsync(Embed);
		}
		
		LogInfo($"{Guild.Name} ({Guild.Id}): Re-opened ticket {Name} (ID/{Id}, Channel/{ChannelId}, Closed?/{bClosed})");
	}

	public async Task Close(DiscordGuild Guild, DiscordUser Moderator, DiscordMessage? Message = null, string Reason = "No reason provided")
	{
		if (bClosed) return;
		bClosed = true;

		DiscordChannel Channel = Guild.GetChannel(ChannelId);

		await Channel.ModifyAsync(a => a.Name = $"{Name}{ClosedSuffix}");
		await Task.Delay(TimeSpan.FromSeconds(1.5));
		
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"The ticket was closed {CEmoji.GirBlep}",
			Description = $"\nReason: {Reason}",
			Color = YellowGreen
		};
		Embed.WithFooter($"Moderator: {Moderator.Username}", Moderator.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);
		
		if (Message != null)
		{
			await Message.RespondAsync(Embed);
		}
		else
		{
			await Channel.SendMessageAsync(Embed);
		}
		
		LogInfo($"{Guild.Name} ({Guild.Id}): Closed ticket {Name} (ID/{Id}, Channel/{ChannelId}, Closed?/{bClosed})");
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
						$"Click the button below to close this ticket.\n",
					Color = YellowGreen
				};
				Embed.WithFooter($"Author: {Args.User.Username}", Args.User.AvatarUrl);
				Embed.WithTimestamp(DateTime.UtcNow);

				DiscordComponentEmoji CloseEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Sender, ":lock:"));
				DiscordButtonComponent CloseButton = new DiscordButtonComponent(ButtonStyle.Primary, "SID_CloseTicket", "Close", false, CloseEmoji);
				
				DiscordComponentEmoji OpenEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Sender, ":key:"));
				DiscordButtonComponent OpenButton = new DiscordButtonComponent(ButtonStyle.Primary, "SID_OpenTicket", "Re-open (Mod-only)", false, OpenEmoji);
				
				DiscordComponentEmoji DeleteEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Sender, ":warning:"));
				DiscordButtonComponent DeleteButton = new DiscordButtonComponent(ButtonStyle.Primary, "SID_DeleteTicket", "Delete (Admin-only)", false, DeleteEmoji);

				DiscordMessageBuilder MessageEmbed = new DiscordMessageBuilder();
				MessageEmbed.WithContent($"{Args.User.Mention} Welcome!");
				MessageEmbed.WithEmbed(Embed);
				MessageEmbed.AddComponents([CloseButton, OpenButton, DeleteButton]);

				DiscordMessage Message = await Channel.SendMessageAsync(MessageEmbed);
				await Message.PinAsync();

				Tickets.Add(new CTicket(Name, Id, Channel.Id, Args.Guild.Id, false));
				LogInfo($"Created new ticket ({Name}/{Id}, Channel/{Channel.Id}, Guild/{Args.Guild.Id})");

				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
			case "SID_CloseTicket":
			{
				CTicket Ticket = Tickets.Find(t => t.ChannelId == Args.Channel.Id);
				Debug.Assert(Ticket != null);
				
				await Ticket.Close(Args.Guild, Args.User);
				
				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
			case "SID_OpenTicket":
			{
				CTicket Ticket = Tickets.Find(t => t.ChannelId == Args.Channel.Id);
				Debug.Assert(Ticket != null);

				if (CanModerate(Args.User as DiscordMember))
				{
					await Ticket.Open(Args.Guild, Args.User);
				}

				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
			case "SID_DeleteTicket":
			{
				CTicket Ticket = Tickets.Find(t => t.ChannelId == Args.Channel.Id);
				Debug.Assert(Ticket != null);
				
				if (IsAdmin(Args.User as DiscordMember))
				{
					await DeleteTicket(Args.Guild, Ticket);
				}

				await Args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			} break;
		}
	}

	public static async Task DeleteTicket(DiscordGuild Guild, CTicket Ticket)
	{
		DiscordChannel Channel = Guild.GetChannel(Ticket.ChannelId);
		await Channel.DeleteAsync();
		
		Tickets.Remove(Ticket);
		
		LogInfo($"{Guild.Name} ({Guild.Id}): Deleted ticket {Ticket.Name} (ID/{Ticket.Id}, Channel/{Ticket.ChannelId}, Closed?/{Ticket.bClosed})");
	}

	public static bool IsTicketChannel(DiscordChannel Channel)
	{
		return Channel.Type == ChannelType.Text && Channel.Name.Contains(CTicket.Prefix) && Channel.Id != CChannel.TicketCreator;
	}
}
