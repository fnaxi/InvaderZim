// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Gir.ID;

namespace Gir.Commands;

public class CTestCommands : BaseCommandModule
{
	[Command("emojis")]
	[RequireUserPermissions(Permissions.Administrator)]
	public async Task Emojis(CommandContext Context)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "Emojis test",
			Description = $"gir_dance: {CEmoji.GirDance} \n gir_blep: {CEmoji.GirBlep} \n gir_dress: {CEmoji.GirDress}",
			Color = YellowGreen
		};
		await Context.RespondAsync(Embed);
	}
}
