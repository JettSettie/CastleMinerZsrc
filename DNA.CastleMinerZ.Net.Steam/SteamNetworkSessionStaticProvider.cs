using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DNA.CastleMinerZ.Net.Steam
{
	public class SteamNetworkSessionStaticProvider : NetworkSessionStaticProvider
	{
		private SteamWorks _steamAPI;

		protected override NetworkSession CreateSession()
		{
			return SteamNetworkSessionProvider.CreateNetworkSession(this, _steamAPI);
		}

		public SteamNetworkSessionStaticProvider(SteamWorks steamAPI)
		{
			_steamAPI = steamAPI;
			_steamAPI.SetOnGameLobbyJoinRequestedCallback(OnJoinLobbyRequested, null);
		}

		private void OnJoinLobbyRequested(ulong lobbyid, ulong inviter, object context)
		{
			InviteAcceptedEventArgs inviteAcceptedEventArgs = new InviteAcceptedEventArgs(Gamer.SignedInGamers[PlayerIndex.One], false);
			inviteAcceptedEventArgs.LobbyId = lobbyid;
			inviteAcceptedEventArgs.InviterId = inviter;
			CallInviteAccepted(inviteAcceptedEventArgs);
		}

		protected override void FinishBeginCreate(BeginCreateSessionState state)
		{
			state.Session.StartHost(state);
			if (state.SessionType != 0)
			{
				_steamAPI.AllowMinimalUpdates = false;
				base.TaskScheduler.QueueUserWorkItem(WaitForHostToStart, state);
			}
		}

		private void WaitForHostToStart(object state)
		{
			try
			{
				BeginCreateSessionState beginCreateSessionState = (BeginCreateSessionState)state;
				while ((beginCreateSessionState.Session.HostConnectionResult != 0 || beginCreateSessionState.Session.LocalGamers.Count <= 0) && beginCreateSessionState.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
				{
					Thread.Sleep(100);
					beginCreateSessionState.Session.Update();
				}
				if (beginCreateSessionState.ExceptionEncountered == null)
				{
					beginCreateSessionState.ExceptionEncountered = new Exception("Unable to start steam lobby");
				}
				TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
				{
					BeginCreateSessionState beginCreateSessionState2 = obj as BeginCreateSessionState;
					beginCreateSessionState2.Event.Set();
					if (beginCreateSessionState2.Callback != null)
					{
						beginCreateSessionState2.Callback(beginCreateSessionState2);
					}
				}, beginCreateSessionState);
			}
			finally
			{
				_steamAPI.AllowMinimalUpdates = true;
			}
		}

		protected override void FinishBeginJoinInvited(ulong lobbyId, BeginJoinSessionState state, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			state.Session.StartClientInvited(lobbyId, state, getPasswordCallback);
			_steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(WaitForClientToStart, state);
		}

		protected override void FinishBeginJoin(BeginJoinSessionState state)
		{
			state.Session.StartClient(state);
			_steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(WaitForClientToStart, state);
		}

		private void WaitForClientToStart(object state)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				BeginJoinSessionState beginJoinSessionState = (BeginJoinSessionState)state;
				int num = 4;
				NetworkSession.ResultCode resultCode = NetworkSession.ResultCode.Pending;
				string text = "";
				while (true)
				{
					if (beginJoinSessionState.Session.HostConnectionResult == NetworkSession.ResultCode.Succeeded && beginJoinSessionState.Session.LocalGamers.Count > 0)
					{
						resultCode = beginJoinSessionState.Session.HostConnectionResult;
						text = beginJoinSessionState.Session.HostConnectionResultString;
						break;
					}
					if (stopwatch.Elapsed.TotalSeconds > 15.0)
					{
						if (num == 0)
						{
							resultCode = NetworkSession.ResultCode.Timeout;
							text = "Server or Steam is not responding";
							break;
						}
						num--;
						beginJoinSessionState.Session.ResetHostConnectionResult();
						beginJoinSessionState.Session.StartClient(beginJoinSessionState);
						stopwatch.Restart();
					}
					if (beginJoinSessionState.Session.HostConnectionResult > NetworkSession.ResultCode.Succeeded)
					{
						resultCode = beginJoinSessionState.Session.HostConnectionResult;
						text = beginJoinSessionState.Session.HostConnectionResultString;
						break;
					}
					Thread.Sleep(100);
					beginJoinSessionState.Session.Update();
				}
				beginJoinSessionState.HostConnectionResult = resultCode;
				beginJoinSessionState.HostConnectionResultString = text;
				TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
				{
					BeginJoinSessionState beginJoinSessionState2 = obj as BeginJoinSessionState;
					beginJoinSessionState2.Event.Set();
					if (beginJoinSessionState2.Callback != null)
					{
						beginJoinSessionState2.Callback(beginJoinSessionState2);
					}
				}, beginJoinSessionState);
			}
			finally
			{
				_steamAPI.AllowMinimalUpdates = true;
			}
		}

		protected override void FinishBeginFind(SessionQueryState state)
		{
			state.Sessions = null;
			state.ClientSessionsFound = null;
			_steamAPI.FindGames(state.SearchProperties, OnSessionsFound, state);
			_steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(WaitForFindToComplete, state);
		}

		protected void OnSessionsFound(List<ClientSessionInfo> clientSessions, object context)
		{
			SessionQueryState sessionQueryState = (SessionQueryState)context;
			sessionQueryState.ClientSessionsFound = new List<ClientSessionInfo>(clientSessions);
		}

		private void WaitForFindToComplete(object state)
		{
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				SessionQueryState sessionQueryState = (SessionQueryState)state;
				while (sessionQueryState.ClientSessionsFound == null)
				{
					Thread.Sleep(100);
					_steamAPI.Update();
					if (stopwatch.Elapsed.TotalSeconds > 5.0)
					{
						_steamAPI.StopFindingGames();
					}
				}
				List<AvailableNetworkSession> list = new List<AvailableNetworkSession>();
				foreach (ClientSessionInfo item in sessionQueryState.ClientSessionsFound)
				{
					list.Add(new AvailableNetworkSession(item));
				}
				sessionQueryState.Sessions = new AvailableNetworkSessionCollection(list);
				sessionQueryState.ClientSessionsFound.Clear();
				sessionQueryState.ClientSessionsFound = null;
				sessionQueryState.Event.Set();
				if (sessionQueryState.Callback != null)
				{
					sessionQueryState.Callback(sessionQueryState);
				}
			}
			finally
			{
				_steamAPI.AllowMinimalUpdates = true;
			}
		}

		public override HostDiscovery GetHostDiscoveryObject(string gamename, int version, PlayerID playerID)
		{
			return new SteamHostDiscovery(_steamAPI, gamename, version, playerID);
		}
	}
}
