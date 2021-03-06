using DNA.Distribution.Steam;
using DNA.Net.MatchMaking;
using System;

namespace DNA.CastleMinerZ.Net.Steam
{
	public class SteamNetworkSessionServices : NetworkSessionServices
	{
		private SteamWorks _steamAPI;

		public SteamNetworkSessionServices(SteamWorks steamAPI, Guid productID, int networkVersion)
			: base(productID, networkVersion)
		{
			_steamAPI = steamAPI;
		}

		public override void ReportSessionAlive(HostSessionInfo hostSession)
		{
		}

		public override HostSessionInfo CreateNetworkSession(CreateSessionInfo sessionInfo)
		{
			return null;
		}

		public override void CloseNetworkSession(HostSessionInfo hostSession)
		{
		}

		public override void UpdateHostSession(HostSessionInfo hostSession)
		{
		}

		public override void UpdateClientInfo(ClientSessionInfo clientSession)
		{
		}

		public override void ReportClientJoined(HostSessionInfo hostSession, string userName)
		{
		}

		public override void ReportClientLeft(HostSessionInfo hostSession, string userName)
		{
		}

		public override ClientSessionInfo[] QueryClientInfo(QuerySessionInfo queryInfo)
		{
			return null;
		}
	}
}
