// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace InvaderZim.Commands;

public class CTestCommands : BaseCommandModule
{
	[Command("ban")]
	public async Task Ban(CommandContext Context)
	{
		await Context.Channel.SendMessageAsync("You are banned!");
	}
}
