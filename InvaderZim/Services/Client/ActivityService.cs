// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.Misc;

namespace InvaderZim.Services.Client;

public class CActivityService
{
	private readonly DiscordClient Client;
	public CActivityService(DiscordClient InClient)
	{
		Client = InClient;
		Client.Ready += Client_OnReady;
	}
	
	private async Task Client_OnReady(DiscordClient Sender, ReadyEventArgs Args)
	{
		DiscordEmoji WaffleEmoji = DiscordEmoji.FromName(Sender, ":waffle:");
		DiscordEmoji MonsterEmoji = DiscordEmoji.FromName(Sender, ":cocktail:");
		DiscordEmoji ConquestEmoji = DiscordEmoji.FromName(Sender, ":earth_americas:");

		const UInt16 UpdateTime = 30;
		List<string> Statuses =
		[
			$"{WaffleEmoji} Eating tasty waffles",
			$"{MonsterEmoji} Drinking a white Monster",
			$"{ConquestEmoji} Conquering the world!"
		];

		CLog.Info($"Status rotation loop started (updating every {UpdateTime} seconds)");
		while ( !(Sender.Ping > 3000) ) // TODO: Ambitious
		{
			try
			{
				string Status = RandomString(Statuses);

				await UpdateActivity(new DiscordActivity(Status, ActivityType.Playing), UserStatus.Online);
			}
			catch (Exception Ex)
			{
				CLog.Error($"Failed to update status: {Ex.Message}");
			}
			
			await Task.Delay(TimeSpan.FromSeconds(UpdateTime)); 
		}
	}
	
	private async Task UpdateActivity(DiscordActivity Activity, UserStatus Status)
	{
		await Client.UpdateStatusAsync(Activity, Status);
		CLog.Info($"Bot activity updated: {Activity.ActivityType} / {Activity.Name}");
	}
}