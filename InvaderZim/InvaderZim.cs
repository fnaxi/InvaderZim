// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InvaderZim.Commands;
using InvaderZim.Config;
using InvaderZim.ID;
using InvaderZim.Misc;

namespace InvaderZim;

public static class CInvaderZim
{
	// TODO: Fix warnings here
	private static DiscordClient Client { get; set; }
	private static CommandsNextExtension Commands { get; set; }

	private static CConfig Config;
	private static DiscordConfiguration DisConfig;
	
	public static async Task Main()
	{
		Config = CConfigParser.Parse();
		DisConfig = new DiscordConfiguration
		{
			Intents = DiscordIntents.All,
			
			Token = Config.Token,
			TokenType = TokenType.Bot,
			AutoReconnect = true
		};
		
		Client =  new DiscordClient(DisConfig);
		Client.Ready += Client_OnReady;
		Client.MessageCreated += Client_OnMessageCreated;
		Client.GuildMemberAdded += Client_OnGuildMemberAdded;
		Client.GuildMemberRemoved += Client_OnGuildMemberRemoved;
		// TODO: Modlog (deleted/edited messages, etc.)
		
		SetupCommands(Config.Prefix);
		
		await Client.ConnectAsync();
		await Task.Delay(-1);
	}

	private static async Task Client_OnGuildMemberAdded(DiscordClient Sender, GuildMemberAddEventArgs Args)
	{
		DiscordChannel RulesChannel = Args.Guild.GetChannel(CChannel.Rules);
		
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName}, welcome to the server! {CEmoji.GirDress}",
			Description = $"{RandomString(CQuote.Arrive)}, {Args.Member.Mention}! Zim is glad to see you here! Make sure to read {RulesChannel.Mention}",
			Color = YellowGreen
		};
		// TODO: Embed.WithImageUrl("");
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);
	}
	
	private static async Task Client_OnGuildMemberRemoved(DiscordClient Sender, GuildMemberRemoveEventArgs Args)
	{
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = $"{Args.Member.DisplayName} has left the server {CEmoji.GirBlep}",
			Description = $"{RandomString(CQuote.Left)}! Zim is sad to see you go",
			Color = YellowGreen
		};
		Embed.WithThumbnail(Args.Member.AvatarUrl);
		Embed.WithTimestamp(DateTime.UtcNow);

		DiscordChannel WelcomeChannel = Args.Guild.GetChannel(CChannel.Welcome);
		await WelcomeChannel.SendMessageAsync(Embed);
	}

	private static async Task Client_OnReady(DiscordClient Sender, ReadyEventArgs Args)
	{
		CLog.Info("Zim is ready!");
		
		// TODO: remove later
		DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
		{
			Title = "Zim is eating waffles again!",
			Description = "Prepare your bladder for imminent release!",
			Color = YellowGreen
		};
		
		DiscordChannel Channel = await Sender.GetChannelAsync(CChannel.Test);
		await Channel.SendMessageAsync(Embed);
		
		await StartStatusRotation(Sender);
	}
	
	private static async Task Client_OnMessageCreated(DiscordClient Sender, MessageCreateEventArgs Args)
	{
		if (Args.Author.IsBot) return;
		
		if (Args.Message.MentionedUsers.Any(uz => uz.Id == Sender.CurrentUser.Id))
			// TODO: Bot should react to mentioning "zim" in a message
		{
			string Message = Args.Message.Content;
			if (Regex.IsMatch(Message, @"\b(hi|hey|hello)\b", RegexOptions.IgnoreCase))
			{
				await Args.Message.RespondAsync(RandomString(CQuote.Hello));
			}
			else
			{
				await Args.Message.RespondAsync(RandomString(CQuote.Mention));
			}
		}
	}
	
	private static async Task Commands_OnCommandErrored(CommandsNextExtension Sender, CommandErrorEventArgs Args)
	{
		if (Args.Exception is ArgumentException or DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)
		{
			// TODO: Handle more cases here
			if (CanModerate(Args.Context))
			{
				if (Args.Command?.Name == "ban")
				{
					await Args.Context.RespondAsync("I can't find a member with such username or ID!");
				}
			}
		}
	}

	private static void SetupCommands(string Prefix)
	{
		CommandsNextConfiguration CommandsConfig = new CommandsNextConfiguration()
		{
			StringPrefixes = [Prefix],
			
			EnableMentionPrefix = false,
			EnableDms = false,
			
			EnableDefaultHelp = false
		};
		Commands = Client.UseCommandsNext(CommandsConfig);
		Commands.CommandErrored += Commands_OnCommandErrored;

		RegisterCommandModule<CMiscCommands>();
		RegisterCommandModule<CModerationCommands>();
		RegisterCommandModule<CEntertainCommands>();
		RegisterCommandModule<CTestCommands>();
	}
	
	private static async Task StartStatusRotation(DiscordClient Sender)
	{
		// TODO: See if custom emoji can be used here
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

				await UpdateStatus(new DiscordActivity(Status, ActivityType.Playing), UserStatus.Online);
			}
			catch (Exception Ex)
			{
				CLog.Error($"Failed to update status: {Ex.Message}");
			}
			
			await Task.Delay(TimeSpan.FromSeconds(UpdateTime)); 
		}
	}
	
	private static async Task UpdateStatus(DiscordActivity Activity, UserStatus Status)
	{
		await Client.UpdateStatusAsync(Activity, Status);
		CLog.Info($"Bot status updated: {Activity.ActivityType} / {Activity.Name}");
	}
	
	private static void RegisterCommandModule<T>() where T : BaseCommandModule
	{
		Commands.RegisterCommands<T>();
		CLog.Info($"Registered {typeof(T).Name}");
	}
}
