// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gir.ID;
using Gir.Misc;

namespace Gir.Services.Client;

public class CGreetingService
{
	public CGreetingService(DiscordClient Client)
	{
		Client.GuildMemberAdded += Client_OnGuildMemberAdded;
		Client.GuildMemberRemoved += Client_OnGuildMemberRemoved;
	}
	
	private async Task Client_OnGuildMemberAdded(DiscordClient Sender, GuildMemberAddEventArgs Args)
	{
		DiscordChannel RulesChannel = Args.Guild.GetChannel(CChannel.Rules);
		
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName}, welcome to the server! {CEmoji.GirDress}",
			Description = $"{RandomString(CQuote.Arrive)}, {Args.Member.Mention}! Gir is glad to see you here! \n Make sure to read the {RulesChannel.Mention}",
			Color = YellowGreen
		};
		// TODO: Embed.WithImageUrl("");
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);

		DiscordRole MemberRole = Args.Guild.GetRole(CRole.Member);
		await Args.Member.GrantRoleAsync(MemberRole);
	}
	
	private async Task Client_OnGuildMemberRemoved(DiscordClient Sender, GuildMemberRemoveEventArgs Args)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName} has left the server {CEmoji.GirBlep}",
			Description = $"{RandomString(CQuote.Left)}! Gir is sad to see you go",
			Color = YellowGreen
		};
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);
	}
}
