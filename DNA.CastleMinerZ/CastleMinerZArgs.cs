using DNA.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DNA.CastleMinerZ
{
	internal class CastleMinerZArgs : CommandLineArgs
	{
		public static CastleMinerZArgs Instance;

		public ulong InvitedLobbyID;

		public bool ForceReachProfile;

		public string TextureFolder;

		public string TextureDumpFolderName;

		public bool ShowInventoryAtlas;

		public int TextureQualityLevel = 1;

		protected void LobbyArgHandler(string flag, List<string> args)
		{
			string text = args[0];
			if (!ulong.TryParse(text, out InvitedLobbyID))
			{
				ErrorString = "Couldn't parse lobby id: " + text;
				ShowUsage = true;
			}
			else if (InvitedLobbyID == 0)
			{
				ErrorString = "Invalid lobby id: " + text;
				ShowUsage = true;
			}
		}

		protected void UserTexturesHandler(string flag, List<string> args)
		{
			if (args == null || args.Count != 1)
			{
				TextureFolder = GlobalSettings.GetAppDataDirectory("Textures");
				if (!Directory.Exists(TextureFolder))
				{
					Directory.CreateDirectory(TextureFolder);
				}
			}
			else if (!Directory.Exists(args[0]))
			{
				ErrorString = "texture_folder argument is not a folder: " + args[0];
				ShowUsage = true;
			}
			else
			{
				TextureFolder = args[0];
			}
		}

		protected void DumpTexturesHandler(string flag, List<string> args)
		{
			if (args == null || args.Count != 1)
			{
				ErrorString = "Invalid argument for dump_textures (do you need quotes?):";
				ShowUsage = true;
			}
			else if (!Directory.Exists(args[0]))
			{
				ErrorString = "dump_textures does not specify an existing folder: " + args[0];
				ShowUsage = true;
			}
			else
			{
				TextureDumpFolderName = args[0];
			}
		}

		protected void ShowInventoryAtlasHandler(string flag, List<string> args)
		{
			ShowInventoryAtlas = true;
		}

		protected void ForceReachHandler(string flag, List<string> args)
		{
			ForceReachProfile = true;
		}

		protected void HelpArgHandler(string flag, List<string> args)
		{
			ShowUsage = true;
		}

		protected void TextureQualityHandler(string flag, List<string> args)
		{
			if (args.Count > 0)
			{
				switch (args[0].ToLower())
				{
				case "high":
					TextureQualityLevel = 1;
					break;
				case "med":
					TextureQualityLevel = 2;
					break;
				case "low":
					TextureQualityLevel = 3;
					break;
				}
			}
		}

		protected void ForceLanguageHandler(string flag, List<string> args)
		{
			if (args.Count > 0)
			{
				switch (args[0].ToLower())
				{
				case "english":
				{
					CultureInfo cultureInfo24 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"));
					break;
				}
				case "french":
				{
					CultureInfo cultureInfo21 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"));
					break;
				}
				case "german":
				{
					CultureInfo cultureInfo18 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("de"));
					break;
				}
				case "italian":
				{
					CultureInfo cultureInfo15 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("it"));
					break;
				}
				case "japanese":
				{
					CultureInfo cultureInfo12 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja"));
					break;
				}
				case "portuguese":
				{
					CultureInfo cultureInfo9 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt"));
					break;
				}
				case "russian":
				{
					CultureInfo cultureInfo6 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru"));
					break;
				}
				case "spanish":
				{
					CultureInfo cultureInfo3 = Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("es"));
					break;
				}
				}
			}
		}

		public CastleMinerZArgs()
		{
			AddFlag("+connect_lobby", 1, LobbyArgHandler, "Steam lobby ID to join on launch ", "[+connect_lobby steam_lobby_id]");
			AddFlag("+reach", 0, ForceReachHandler, "Force reach graphics profile", "[+reach]");
			AddFlag("+texture_quality", 1, TextureQualityHandler, "Texture quality (high,med,low)", "[+texture_quality [high|med|low]]");
			AddFlag("+texture_folder", -2, UserTexturesHandler, "Look for user supplied textures (default is AppData\\Local\\CastleMinerZ\\Textures)", "[+texture_folder [path_to_folder]]");
			AddFlag("+language", -2, ForceLanguageHandler, "Force the game into a language", "[+language [language]]");
			AddFlag("+dump_textures", 1, DumpTexturesHandler, "The game will dump all loaded textures to the given directory", "[+dump_textures path_to_folder]");
			AddFlag("+show_inv_atlas", 0, ShowInventoryAtlasHandler, null, null);
			AddFlag("-h", 0, HelpArgHandler, "Show this usage message", "[-h]");
			AddFlag("-?", 0, HelpArgHandler, "Show this usage message", "[-?]");
			Instance = this;
		}
	}
}
