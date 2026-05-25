// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InvaderZim.ID;

namespace InvaderZim.Commands;

public class CTestCommands : BaseCommandModule
{
	[Command("emojis")] [RequireUserPermissions(Permissions.Administrator)]
	public async Task Emojis(CommandContext Context)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "Emojis test",
			Description = $"{CEmoji.GirDance} \n {CEmoji.GirBlep} \n {CEmoji.GirDress} \n {CEmoji.GirLaugh} \n {CEmoji.GirLike}",
			Color = YellowGreen
		};
		await Context.Channel.SendMessageAsync(Embed);
	}

	[Command("embed")] [RequireUserPermissions(Permissions.Administrator)]
	public async Task Embed(CommandContext Context)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "FEEL HONORED!",
			Description = "- I have come to accept your feelings for me. I congratulate you for acknowledging my superiority in choosing me as your love-pig. FEEL HONORED!",
			Color = YellowGreen
		};
		await Context.Channel.SendMessageAsync(Embed);
	}
}
