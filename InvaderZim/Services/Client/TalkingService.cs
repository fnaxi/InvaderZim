// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using InvaderZim.Misc;

namespace InvaderZim.Services.Client;

public class CTalkingService
{
	private readonly DiscordClient Client;
	public CTalkingService(DiscordClient InClient)
	{
		Client = InClient;
		Client.MessageCreated += Client_OnMessageCreated;
	}
	
	private async Task Client_OnMessageCreated(DiscordClient Sender, MessageCreateEventArgs Args)
	{
		if (Args.Author.IsBot) return;

		// TODO: Bot should react to mentioning "zim" in a message
		
		if (Args.Message.MentionedUsers.Any(uz => uz.Id == Sender.CurrentUser.Id))
		{
			if (Regex.IsMatch(Args.Message.Content, @"\b(hi|hey|hello)\b", RegexOptions.IgnoreCase))
			{
				await Args.Message.RespondAsync(RandomString(CQuote.Hello));
			}
			else
			{
				await Args.Message.RespondAsync(RandomString(CQuote.Mention));
			}
		}
	}
}
