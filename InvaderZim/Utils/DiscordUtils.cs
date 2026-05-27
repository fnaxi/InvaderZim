// CopyRight https://github.com/fnaxi. All Rights Reserved.

global using static InvaderZim.Utils.CDiscordUtils;
using DSharpPlus.Entities;
using InvaderZim.ID;

namespace InvaderZim.Utils;

public static class CDiscordUtils
{
	public static bool IsTempVoice(DiscordChannel Channel)
	{
		return Channel.Id is not (CChannel.CreateVoice or CChannel.AFK);
	}
}
