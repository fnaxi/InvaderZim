// CopyRight https://github.com/fnaxi. All Rights Reserved.

using Newtonsoft.Json;

namespace InvaderZim.Config;

public class CConfig
{
	// TODO: Revisit this
	public string Token { get; set; }
	public string Prefix { get; set; }
}

public class CConfigReader
{
	public string Token { get; set; }
	public string Prefix { get; set; }
	
	private const string Name = "Config.json";

	public void Parse()
	{
		StreamReader Stream = new StreamReader(Name);
		string Json = Stream.ReadToEnd();

		CConfig? Config = JsonConvert.DeserializeObject<CConfig>(Json);
		Verify(Config != null, "Failed to read config!");
		
		Token = Config.Token;
		Prefix = Config.Prefix;
		
		CLog.Info("Parsed config");
		CLog.Info($"Token: {Token}");
		CLog.Info($"Prefix: {Prefix}");
	}
}
