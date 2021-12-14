using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ.Utils
{
	public class DebugUtils
	{
		private static bool broadLogMessages;

		public static void Log(string message)
		{
			if (broadLogMessages)
			{
				BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, "[" + CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag + "] " + message);
			}
		}
	}
}
