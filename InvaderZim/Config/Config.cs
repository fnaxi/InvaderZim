// CopyRight https://github.com/fnaxi. All Rights Reserved.

using System.Diagnostics;
using InvaderZim.Misc;
using Newtonsoft.Json;

namespace InvaderZim.Config;

public class CConfig(string InToken)
{
	public string Token = InToken;
	public const string Prefix = "gir ";
	
	private const string Name = "Config.json";
	public static CConfig Parse() // TODO: Move IDs to Config.json
	{
		StreamReader Stream = new StreamReader(Name);
		string Json = Stream.ReadToEnd();
		Stream.Close();

		CConfig? Config = JsonConvert.DeserializeObject<CConfig>(Json);
		Debug.Assert(Config != null, nameof(Config) + " != null");
		Debug.Assert(IsTextValid(Config.Token), "Config was not parsed properly or it didn't set required fields!");
		
		LogInfo("Parsed config");
		LogDebug($"Token: {Config.Token}");
		LogInfo($"Prefix: '{Prefix}'");
		
		return Config;
	}
}
