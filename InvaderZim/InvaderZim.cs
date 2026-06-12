// CopyRight https://github.com/fnaxi. All Rights Reserved.

using DSharpPlus;
using DSharpPlus.CommandsNext;
using InvaderZim.Commands;
using InvaderZim.Config;
using InvaderZim.Services.Client;
using InvaderZim.Services.Client.System;
using InvaderZim.Services.Commands;
using Microsoft.Extensions.Logging;

namespace InvaderZim;

public class CInvaderZim
{
	private static DiscordClient Client = null!;
	private static CommandsNextExtension Commands { get; set; } = null!;
	
	/*----------------------------------------------------------------------------
		Client services
	----------------------------------------------------------------------------*/
	private static CTemporaryVoicesService TempVoicesService = null!;
	private static CGreetingService GreetingService = null!;
	private static CTalkingService TalkingService = null!;
	private static CActivityService ActivityService = null!;
	private static CColorRolesService ColorRolesService = null!;
	private static CModerationLogService ModerationLogService = null!;
	public  static CTicketsService TicketsService = null!;
	public  static CHelpMenuService HelpMenuService = null!;
	
	/*----------------------------------------------------------------------------
		System client services
	----------------------------------------------------------------------------*/
	private static CConnectionStatusService ConnectionStatusService = null!;
	
	/*----------------------------------------------------------------------------
		Commands Services
	----------------------------------------------------------------------------*/
	private static CCommandErrorsService CommandErrorsService = null!;
	
	public const LogLevel MinimumLogLevel = LogLevel.Information;
	
	public static async Task Main()
	{
		CConfig Config = CConfigParser.Parse();
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
		
		SetupCommands(Config.Prefix);
		SetupServices();

		await Client.ConnectAsync();
		await Task.Delay(-1);
	}
	
	private static void SetupServices()
	{
		// Client
		TempVoicesService = new CTemporaryVoicesService(Client);
		GreetingService = new CGreetingService(Client);
		TalkingService = new CTalkingService(Client);
		ActivityService = new CActivityService(Client);
		ColorRolesService = new CColorRolesService(Client);
		ModerationLogService = new CModerationLogService(Client);
		TicketsService = new CTicketsService(Client);
		HelpMenuService = new CHelpMenuService(Client);
		
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
