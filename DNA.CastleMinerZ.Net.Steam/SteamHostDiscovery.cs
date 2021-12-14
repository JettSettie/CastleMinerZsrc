using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using System.Diagnostics;

namespace DNA.CastleMinerZ.Net.Steam
{
	internal class SteamHostDiscovery : HostDiscovery
	{
		private SteamWorks _steamAPI;

		public SteamHostDiscovery(SteamWorks steam, string gamename, int version, PlayerID playerID)
			: base(gamename, version, playerID)
		{
			_steamAPI = steam;
			_steamAPI.SetOnGameUpdatedCallback(SessionUpdatedCallback, null);
			Timeout = 10f;
		}

		public override int GetHostInfo(ulong lobbyid, HostDiscoveryCallback callback, object context)
		{
			int num = _nextWaitingID++;
			WaitingForResponse waitingForResponse = new WaitingForResponse();
			waitingForResponse.Callback = callback;
			waitingForResponse.Context = context;
			waitingForResponse.WaitingID = num;
			waitingForResponse.SteamLobbyID = lobbyid;
			_awaitingResponse.Add(waitingForResponse);
			waitingForResponse.Timer = Stopwatch.StartNew();
			_steamAPI.GetUpdatedGameInfo(lobbyid);
			return num;
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			WaitingForResponse waitingForResponse = FindWaiterBySteamID(lobbyid);
			if (waitingForResponse != null)
			{
				AvailableNetworkSession session2 = null;
				ResultCode result = ResultCode.ConnectionDenied;
				switch (updateresult)
				{
				case GameUpdateResultCode.Success:
					session2 = new AvailableNetworkSession(session);
					result = ResultCode.Success;
					break;
				case GameUpdateResultCode.NoLongerValid:
					result = ResultCode.ConnectionDenied;
					break;
				case GameUpdateResultCode.UnknownGame:
					result = ResultCode.ConnectionDenied;
					break;
				}
				waitingForResponse.Callback(result, session2, waitingForResponse.Context);
			}
		}

		private WaitingForResponse FindWaiterBySteamID(ulong lobbyId)
		{
			lock (_awaitingResponse)
			{
				for (int i = 0; i < _awaitingResponse.Count; i++)
				{
					if (_awaitingResponse[i].SteamLobbyID == lobbyId)
					{
						WaitingForResponse result = _awaitingResponse[i];
						_awaitingResponse.RemoveAt(i);
						return result;
					}
				}
			}
			return null;
		}

		public override void Update()
		{
			base.Update();
		}

		public override void Shutdown()
		{
			_steamAPI.ClearOnGameUpdatedCallback();
		}
	}
}
