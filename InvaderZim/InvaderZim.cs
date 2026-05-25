// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using InvaderZim.Commands;
using InvaderZim.Config;

namespace InvaderZim;

public class CInvaderZim
{
	private static DiscordClient Client { get; set; }
	private static CommandsNextExtension Commands { get; set; }
	
	public static async Task Main()
	{
		CConfigReader Config = new CConfigReader();
		{
			Config.Parse();
		}
		DiscordConfiguration DisConfig = new DiscordConfiguration()
		{
			Intents = DiscordIntents.All,
			
			Token = Config.Token,
			TokenType = TokenType.Bot,
			AutoReconnect = true
		};
		
		Client =  new DiscordClient(DisConfig);
		Client.Ready += ClientOnReady;

		SetupCommands(Config.Prefix);
		
		await Client.ConnectAsync();
		await Task.Delay(-1);
	}

	private static void SetupCommands(string Prefix)
	{
		CommandsNextConfiguration CommandsConfig = new CommandsNextConfiguration()
		{
			StringPrefixes = [Prefix],
			
			EnableMentionPrefix = true,
			EnableDms = false,
			
			EnableDefaultHelp = false
		};
		Commands = Client.UseCommandsNext(CommandsConfig);

		Commands.RegisterCommands<CTestCommands>();
		CLog.Info("Registered CTestCommands");
		
		Commands.RegisterCommands<CModerationCommands>();
		CLog.Info("Registered CModerationCommands");
	}

	private static Task ClientOnReady(DiscordClient Sender, ReadyEventArgs Args)
	{
		return Task.CompletedTask;
	}
}
