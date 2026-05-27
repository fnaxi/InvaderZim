// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim.Services.Client.System;

public class CConnectionStatusService
{
	private readonly DiscordClient Client;
	public CConnectionStatusService(DiscordClient InClient)
	{
		Client = InClient;
		Client.Ready += Client_OnReady;
	}
	
	private async Task Client_OnReady(DiscordClient Sender, ReadyEventArgs Args)
	{
		CLog.Info("Zim is ready!");
		
		// TODO: remove later
		{
			DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
			{
				Title = "Zim is eating waffles again!",
				Description = "Prepare your bladder for imminent release!",
				Color = YellowGreen
			};

			DiscordChannel Channel = await Sender.GetChannelAsync(CChannel.Test);
			await Channel.SendMessageAsync(Embed);
		}
	}
}
