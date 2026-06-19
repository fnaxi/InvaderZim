// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Gir.Services.Client;

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
		DiscordEmoji MonsterEmoji = DiscordEmoji.FromName(Sender, ":coffee:");
		DiscordEmoji ConquestEmoji = DiscordEmoji.FromName(Sender, ":crossed_swords:");

		const UInt16 UpdateTime = 720;
		List<string> Statuses =
		[
			$"{WaffleEmoji} Eating tasty waffles",
			$"{MonsterEmoji} Drinking a white Monster",
			$"{ConquestEmoji} Conquering the world!"
		];

		LogInfo($"Status rotation loop started (updating every {UpdateTime} seconds)");
		while ( !(Sender.Ping > 3000) ) // TODO: Ambitious
		{
			string Status = RandomString(Statuses);

			await UpdateActivity(new DiscordActivity(Status, ActivityType.Playing), UserStatus.Online);
			await Task.Delay(TimeSpan.FromSeconds(UpdateTime)); 
		}
	}
	
	private async Task UpdateActivity(DiscordActivity Activity, UserStatus Status)
	{
		await Client.UpdateStatusAsync(Activity, Status);
		LogInfo($"Bot activity updated: {Activity.ActivityType} / {Activity.Name}");
	}
}