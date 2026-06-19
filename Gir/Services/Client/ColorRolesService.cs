// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gir.ID;

namespace Gir.Services.Client;

public class CColorRolesService
{
	public CColorRolesService(DiscordClient Client)
	{
		Client.GuildDownloadCompleted += Client_OnGuildDownloadCompleted;
		Client.MessageReactionAdded += Client_OnMessageReactionAdded;
		Client.MessageReactionRemoved += Client_OnMessageReactionRemoved;
	}

	private UInt64 MessageId = 1510595309506396181;

	private Dictionary<DiscordEmoji, UInt64> ColorRolesMap = null!;
	
	private async Task Client_OnGuildDownloadCompleted(DiscordClient Sender, GuildDownloadCompletedEventArgs Args)
	{
		ColorRolesMap = new Dictionary<DiscordEmoji, UInt64>
		{
			{ DiscordEmoji.FromName(Sender, ":red_square:"), CRole.Red },
			{ DiscordEmoji.FromName(Sender, ":purple_square:"), CRole.Purple },
			{ DiscordEmoji.FromName(Sender, ":blue_square:"), CRole.Blue },
			{ DiscordEmoji.FromName(Sender, ":green_square:"), CRole.Green },
			{ DiscordEmoji.FromName(Sender, ":yellow_square:"), CRole.Yellow },
			{ DiscordEmoji.FromName(Sender, ":orange_square:"), CRole.Orange }
		};
		
		foreach (DiscordGuild Guild in Sender.Guilds.Values)
		{ 
			DiscordChannel Channel = Guild.GetChannel(CChannel.Info);
			if (Channel == null) return;

			// TODO: message ID change iteration is slow
			if (MessageId == 0)
			{
				LogWarning("MessageId hasn't been set! Color roles service will not work");
				return;
			}
			DiscordMessage Message = await Channel.GetMessageAsync(MessageId);
			
			await CreateReactions(Message);
			// TODO: await HandleOfflineReactions(Guild, Message);
		}
	}

	/** Grants a role to the users who reacted and revokes if ones removed it when bot was offline. */
	private async Task HandleOfflineReactions(DiscordGuild Guild, DiscordMessage Message)
	{
		IReadOnlyCollection<DiscordMember>? Members = await Guild.GetAllMembersAsync();
		foreach (KeyValuePair<DiscordEmoji, UInt64> ColorRole in ColorRolesMap)
		{
			DiscordEmoji Emoji = ColorRole.Key;
			UInt64 RoleId = ColorRole.Value;
			
			LogInfo($"Checking offline reactions for {Emoji.GetDiscordName()} / {RoleId}");
			
			DiscordRole? Role = Guild.GetRole(RoleId);
			if (Role == null)
			{
				LogError($"Cannot find role with Id {RoleId}!");
				continue;
			}

			IReadOnlyList<DiscordUser>? UsersWhoReacted = await Message.GetReactionsAsync(Emoji); // TODO: user limit is 25
			foreach (DiscordMember Member in Members)
			{
				if (Member.IsBot) continue;

				bool bReacted = UsersWhoReacted.Any(uz => uz.Id == Member.Id);
				bool bHasRole = Member.Roles.Contains(Role);

				switch (bReacted)
				{
					case true when !bHasRole:
						await Member.GrantRoleAsync(Role);
						LogInfo($"Granted {Role.Name} / {Role.Id} to {Member.Username} (offline reaction)");
						await Task.Delay(TimeSpan.FromSeconds(2));
						break;
						
					case false when bHasRole:
						await Member.RevokeRoleAsync(Role);
						LogInfo($"Revoked {Role.Name} / {Role.Id} from {Member.Username} (offline reaction)");
						await Task.Delay(TimeSpan.FromSeconds(2));
						break;
				}
			}
			await Task.Delay(TimeSpan.FromSeconds(3));
		}
	}

	/** Create color reactions on a message if there are none. */
	private async Task CreateReactions(DiscordMessage Message)
	{
		foreach (DiscordEmoji Emoji in ColorRolesMap.Keys)
		{
			DiscordReaction? Reaction = Message.Reactions.FirstOrDefault(r => r.Emoji == Emoji);
			if (Reaction != null && Reaction.IsMe) continue;

			await Message.CreateReactionAsync(Emoji);
			await Task.Delay(TimeSpan.FromSeconds(1));
		}
	}

	private async Task Client_OnMessageReactionAdded(DiscordClient Sender, MessageReactionAddEventArgs Args)
	{
		if (Args.Message.Id != MessageId || Args.User.IsBot) return;
		
		if (ColorRolesMap.TryGetValue(Args.Emoji, out UInt64 RoleId))
		{
			if (Args.User is DiscordMember Member)
			{
				DiscordRole? Role = Args.Guild.GetRole(RoleId);
				if (Role != null)
				{
					await Member.GrantRoleAsync(Role);
				}
			}
		}
	}
	
	private async Task Client_OnMessageReactionRemoved(DiscordClient Sender, MessageReactionRemoveEventArgs Args)
	{
		if (Args.Message.Id != MessageId) return;
		
		if (ColorRolesMap.TryGetValue(Args.Emoji, out UInt64 RoleId))
		{
			DiscordMember? Member = await Args.Guild.GetMemberAsync(Args.User.Id);
			DiscordRole? Role = Args.Guild.GetRole(RoleId);
			if (Member != null && Role != null)
			{
				await Member.RevokeRoleAsync(Role);
			}
		}
	}
}