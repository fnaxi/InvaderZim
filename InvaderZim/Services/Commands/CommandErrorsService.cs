// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus.CommandsNext;

namespace InvaderZim.Services.Commands;

public class CCommandErrorsService
{
	private readonly CommandsNextExtension Commands;
	public CCommandErrorsService(CommandsNextExtension InCommands)
	{
		Commands = InCommands;
		Commands.CommandErrored += Commands_OnCommandErrored;
	}
	
	private static async Task Commands_OnCommandErrored(CommandsNextExtension Sender, CommandErrorEventArgs Args)
	{
		if (Args.Exception is ArgumentException or DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)
		{
			// TODO: Handle more cases here
			if (CanModerate(Args.Context))
			{
				// TODO: Revisit this
				if (Args.Command?.Name is "ban" or "kick")
				{
					await Args.Context.RespondAsync("I can't find a member with such username or ID!");
				}
			}
		}
	}
}
