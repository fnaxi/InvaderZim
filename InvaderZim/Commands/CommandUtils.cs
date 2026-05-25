// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Commands.CCommandUtils;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using InvaderZim.ID;

namespace InvaderZim.Commands;

public class CCommandUtils
{
	public static async Task NoRights(CommandContext Context)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "People... they're the worst",
			Description = $"You're not the one who can ask his honor zim{CEmoji.GirBlep} for something like that!",
			Color = YellowGreen
		};
		await Context.Channel.SendMessageAsync(Embed);
	}
	
	public static bool CanModerate(CommandContext Context)
	{
		return Context.Member.Roles.Any(role => role.Id is CRole.Admin or CRole.Moderator);
	}
}
