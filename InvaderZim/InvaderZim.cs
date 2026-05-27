// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using InvaderZim.Commands;
using InvaderZim.Config;
using InvaderZim.Misc;
using InvaderZim.Services.Client;
using InvaderZim.Services.Client.System;
using InvaderZim.Services.Commands;

namespace InvaderZim;

public class CInvaderZim
{
	private DiscordClient Client = null!;
	private CommandsNextExtension Commands { get; set; } = null!;
	
	/*----------------------------------------------------------------------------
		Client services
	----------------------------------------------------------------------------*/
	private CTemporaryVoicesService TempVoicesService = null!;
	private CGreetingService GreetingService = null!;
	private CTalkingService TalkingService = null!;
	private CActivityService ActivityService = null!;
	
	/*----------------------------------------------------------------------------
		System client services
	----------------------------------------------------------------------------*/
	private CConnectionStatusService ConnectionStatusService = null!;
	
	/*----------------------------------------------------------------------------
		Commands Services
	----------------------------------------------------------------------------*/
	private CCommandErrorsService CommandErrorsService = null!;
	
	public async Task Start()
	{
		CConfig Config = CConfigParser.Parse();
		DiscordConfiguration DisConfig = new DiscordConfiguration
		{
			Intents = DiscordIntents.All,
			Token = Config.Token,
			TokenType = TokenType.Bot,
			AutoReconnect = true,
			
			// TODO: Replace with true later
			LogUnknownEvents = false
		};
		
		Client = new DiscordClient(DisConfig);
		
		// TODO: Modlog (deleted/edited messages, etc.)

		SetupCommands(Config.Prefix);
		SetupServices();

		await Client.ConnectAsync();
		await Task.Delay(-1);
	}
	
	private void SetupServices()
	{
		// Client
		TempVoicesService = new CTemporaryVoicesService(Client);
		GreetingService = new CGreetingService(Client);
		TalkingService = new CTalkingService(Client);
		ActivityService = new CActivityService(Client);
		
		// Client.System
		ConnectionStatusService = new CConnectionStatusService(Client);
		
		// Commands
		CommandErrorsService = new CCommandErrorsService(Commands);
	}
	
	private void SetupCommands(string Prefix)
	{
		CommandsNextConfiguration CommandsConfig = new CommandsNextConfiguration()
		{
			StringPrefixes = [Prefix],
			
			IgnoreExtraArguments = true,
			EnableMentionPrefix = false,
			EnableDms = false,
			
			EnableDefaultHelp = false
		};
		Commands = Client.UseCommandsNext(CommandsConfig);

		RegisterCommandModule<CMiscCommands>();
		RegisterCommandModule<CModerationCommands>();
		RegisterCommandModule<CEntertainCommands>();
		RegisterCommandModule<CTestCommands>();
	}
	
	private void RegisterCommandModule<T>() where T : BaseCommandModule
	{
		Commands.RegisterCommands<T>();
		CLog.Info($"Registered {typeof(T).Name}");
	}
}

public abstract class CEntryPoint
{
	public static async Task Main()
	{
		CInvaderZim Bot = new CInvaderZim();
		await Bot.Start();
	}
}
