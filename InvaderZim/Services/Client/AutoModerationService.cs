// CopyRight https://github.com/fnaxi. All Rights Reserved.

//#define MODERATORS_ARE_IMMUNE
#define GIR_HATES_SHORT_VIDEOS

using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.Commands;

namespace InvaderZim.Services.Client;

public class CAutoModerationService
{
	public CAutoModerationService(DiscordClient Client)
	{
		// TODO: anti-raid/join gate
		// Client.GuildMemberAdded
		
		Client.MessageCreated += Client_OnMessageCreated;

		ShortVideoRegex =
		[
			TikTokLinkRegex,
			YouTubeShortsRegex,
			InstagramReelsRegex,
			SnapchatRegex,
			LikeeLinkRegex
		];
	}

	private async Task Client_OnMessageCreated(DiscordClient Sender, MessageCreateEventArgs Args)
	{
		if (Args.Author.IsBot) return;
		
		#if MODERATORS_ARE_IMMUNE
		if (CanModerate(Args.Author as DiscordMember)) return;
		#endif
		
		// TODO: anti-spam
		
		string Content = Args.Message.Content;
		if (DiscordInviteRegex.IsMatch(Content))
		{
			await HandleMessageViolation(Args, "Posted a Discord invite.");
			return;
		}
		#if GIR_HATES_SHORT_VIDEOS
		if (ShortVideoRegex.Any(reg => reg.IsMatch(Content)))
		{
			await HandleMessageViolation(Args, "Posted a short-video link.");
			return;
		}
		#endif
	}

	private async Task HandleMessageViolation(MessageCreateEventArgs Args, string Reason)
	{
		await CModeration.Warn(Args.Guild, Args.Guild.CurrentMember, Args.Author as DiscordMember ?? throw new NullReferenceException(), Args.Message, Reason);
		await Task.Delay(TimeSpan.FromSeconds(0.3));
		await Args.Message.DeleteAsync();
	}

	private readonly Regex[] ShortVideoRegex;
	
	/** discord.gg/uid, discord.com/invite/uid, discordapp.com/invite/uid, etc. */
	private readonly Regex DiscordInviteRegex = 
		new(@"(https?://)?([a-z0-9\-]+\.)?discord(app)?\.(gg|com/invite)/[a-zA-Z0-9\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	/** tiktok.com, vm.tiktok.com, vt.tiktok.com, etc. */
	private readonly Regex TikTokLinkRegex = 
		new(@"(https?://)?([a-z0-9\-]+\.)?tiktok\.com/\S*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	
	/** youtube.com/shorts/uid, youtu.be/uid, etc. */
	private readonly Regex YouTubeShortsRegex = 
		new(@"(https?://)?((youtube\.com/shorts/)|(youtu\.be/))[a-z0-9_\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	
	/** instagram.com/reel/uid, instagram.com/reels/uid, etc. */
	private readonly Regex InstagramReelsRegex = 
		new(@"(https?://)?([a-z0-9\-]+\.)?instagram\.com/reels?/\S*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	
	/** snapchat.com/spotlight/uid, t.snapchat.com/uid, etc. */
	private readonly Regex SnapchatRegex = 
		new(@"(https?://)?((snapchat\.com/(spotlight|add/[a-z0-9_\-\.]+/story)/)|(t\.snapchat\.com/))\S*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	
	/** likee.video/v/uid, l.likee.video/v/uid,  likee.com, etc. */
	private readonly Regex LikeeLinkRegex = 
		new(@"(https?://)?([a-z0-9\-]+\.)?likee\.(video|com)/[a-z0-9_\-/]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
}
