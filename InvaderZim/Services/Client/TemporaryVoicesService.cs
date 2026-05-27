// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;

namespace InvaderZim.Services.Client;

public class CTemporaryVoicesService
{
	private readonly DiscordClient Client;
	public CTemporaryVoicesService(DiscordClient InClient)
	{
		Client = InClient;
		Client.GuildDownloadCompleted += Client_OnGuildDownloadCompleted;
		Client.VoiceStateUpdated += Client_OnVoiceStateUpdated;
	}
	
	private async Task Client_OnGuildDownloadCompleted(DiscordClient Sender, GuildDownloadCompletedEventArgs Args)
	{
		foreach (DiscordGuild? Guild in Sender.Guilds.Values)
		{
			if (Guild == null) continue;

			foreach (DiscordChannel? Channel in Guild.Channels.Values)
			{
				if (Channel == null || Channel.Type != ChannelType.Voice || !IsTempVoice(Channel)) continue;

				await Channel.DeleteAsync();
			}
		}
	}
	
	private const string TempVoiceEmoji = "🜋";
	private async Task Client_OnVoiceStateUpdated(DiscordClient Sender, VoiceStateUpdateEventArgs Args)
	{
		DiscordGuild Guild = Args.Guild;
		
		DiscordChannel CreateVoiceChannel = Guild.GetChannel(CChannel.CreateVoice);
		if (Args.After?.Channel != null && Args.After.Channel.Id == CChannel.CreateVoice)
		{
			DiscordMember Member = await Guild.GetMemberAsync(Args.User.Id);

			string Name = $"{TempVoiceEmoji} {Member.DisplayName}";
			DiscordChannel NewChannel = await Guild.CreateVoiceChannelAsync(Name, Guild.GetChannel(CCategory.VoiceChannels), null, CreateVoiceChannel.UserLimit);

			await Member.ModifyAsync(x => x.VoiceChannel = NewChannel);
		}
		else if (Args.Before?.Channel != null && IsTempVoice(Args.Before.Channel))
		{
			await Args.Before.Channel.DeleteAsync();
		}
	}
	
	private bool IsTempVoice(DiscordChannel Channel)
	{
		return Channel.Id is not (CChannel.CreateVoice or CChannel.AFK);
	}
}
