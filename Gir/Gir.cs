// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using Gir.Commands;
using Gir.Config;
using Gir.Misc;
using Gir.Services.Client;
using Gir.Services.Client.System;
using Gir.Services.Commands;
using Microsoft.Extensions.Logging;

namespace Gir;

public abstract class CGir
{
	private static DiscordClient Client = null!;
	private static CommandsNextExtension Commands { get; set; } = null!;
	
	/*----------------------------------------------------------------------------
		Client services
	----------------------------------------------------------------------------*/
	private static CGreetingService GreetingService = null!;
	private static CTalkingService TalkingService = null!;
	private static CActivityService ActivityService = null!;
	
	private static CHelpMenuService HelpMenuService = null!;
	private static CTemporaryVoicesService TempVoicesService = null!;
	private static CTicketsService TicketsService = null!;
	private static CColorRolesService ColorRolesService = null!;
	
	private static CModerationLogService ModerationLogService = null!;
	private static CAutoModerationService AutoModerationService = null!;
	
	/*----------------------------------------------------------------------------
		System client services
	----------------------------------------------------------------------------*/
	private static CConnectionStatusService ConnectionStatusService = null!;
	
	/*----------------------------------------------------------------------------
		Commands Services
	----------------------------------------------------------------------------*/
	private static CCommandErrorsService CommandErrorsService = null!;
	
	public static async Task Main()
	{
		CConfig Config = CConfig.Parse();
		DiscordConfiguration DisConfig = new DiscordConfiguration
		{
			Intents = DiscordIntents.All,
			
			Token = Config.Token,
			TokenType = TokenType.Bot,
			AutoReconnect = true,
			
			LogUnknownEvents = false, // TODO: Replace with true later
			MinimumLogLevel = MinimumLogLevel,
			LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt"
		};

		Client = new DiscordClient(DisConfig);
		
		SetupCommands(CConfig.Prefix);
		SetupServices();

		await Client.ConnectAsync();
		await Task.Delay(-1);
	}
	
	private static void SetupServices()
	{
		// Client
		GreetingService = new CGreetingService(Client);
		TalkingService = new CTalkingService(Client);
		ActivityService = new CActivityService(Client);

		HelpMenuService = new CHelpMenuService(Client);
		TempVoicesService = new CTemporaryVoicesService(Client);
		TicketsService = new CTicketsService(Client);
		ColorRolesService = new CColorRolesService(Client);
		
		ModerationLogService = new CModerationLogService(Client);
		AutoModerationService = new CAutoModerationService(Client);
		
		// Client.System
		ConnectionStatusService = new CConnectionStatusService(Client);
		
		// Commands
		CommandErrorsService = new CCommandErrorsService(Commands);
	}
	
	private static void SetupCommands(string Prefix)
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
		RegisterCommandModule<CManagementCommands>();
		RegisterCommandModule<CTicketCommands>();
		RegisterCommandModule<CTestCommands>();
	}
	
	private static void RegisterCommandModule<T>() where T : BaseCommandModule
	{
		Commands.RegisterCommands<T>();
		LogInfo($"Registered {typeof(T).Name}");
	}
}
