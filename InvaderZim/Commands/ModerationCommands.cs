// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace InvaderZim.Commands;

public class CModerationCommands : BaseCommandModule
{
	[Command("ban")]
	public async Task Ban(
		CommandContext Context, 
		[Description("The member to ban")] DiscordMember Member,
		[Description("The reason of the ban")] string Reason = "No reason provided")
	{
		if (!CanModerate(Context))
		{
			await NoRights(Context);
			return;
		}
		
		if (Member.Id == Context.Client.CurrentUser.Id || Member.Id == Context.Member.Id)
		{
			await Context.RespondAsync("I had something to say, but you make me forget with your face");
			return;
		}
		
		// TODO: actual ban here
		await Context.RespondAsync("You are banned!");
	}
}
