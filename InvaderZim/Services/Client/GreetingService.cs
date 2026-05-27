// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim.Services.Client;

public class CGreetingService
{
	private readonly DiscordClient Client;
	public CGreetingService(DiscordClient InClient)
	{
		Client = InClient;
		Client.GuildMemberAdded += Client_OnGuildMemberAdded;
		Client.GuildMemberRemoved += Client_OnGuildMemberRemoved;
	}
	
	private async Task Client_OnGuildMemberAdded(DiscordClient Sender, GuildMemberAddEventArgs Args)
	{
		DiscordChannel RulesChannel = Args.Guild.GetChannel(CChannel.Rules);
		
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName}, welcome to the server! {CEmoji.GirDress}",
			Description = $"{RandomString(CQuote.Arrive)}, {Args.Member.Mention}! Zim is glad to see you here! \n Make sure to read the {RulesChannel.Mention}",
			Color = YellowGreen
		};
		// TODO: Embed.WithImageUrl("");
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);
	}
	
	private async Task Client_OnGuildMemberRemoved(DiscordClient Sender, GuildMemberRemoveEventArgs Args)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName} has left the server {CEmoji.GirBlep}",
			Description = $"{RandomString(CQuote.Left)}! Zim is sad to see you go",
			Color = YellowGreen
		};
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);
	}
}
