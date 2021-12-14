using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.Lidgren;
using DNA.Net.MatchMaking;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Net.Steam
{
	public class SteamNetworkSessionProvider : NetworkSessionProvider
	{
		private class KeeperOfTheInvitedGameData
		{
			public GetPasswordForInvitedGameCallback Callback;

			public ClientSessionInfo SessionInfo;

			public ulong LobbyId;

			public NetworkSessionStaticProvider.BeginJoinSessionState State;

			public KeeperOfTheInvitedGameData(GetPasswordForInvitedGameCallback callback, ClientSessionInfo info, ulong id, NetworkSessionStaticProvider.BeginJoinSessionState state)
			{
				Callback = callback;
				SessionInfo = info;
				LobbyId = id;
				State = state;
			}
		}

		private SteamWorks _steamAPI;

		protected int _sessionID;

		protected Dictionary<ulong, Gamer> _steamIDToGamer = new Dictionary<ulong, Gamer>();

		public static NetworkSession CreateNetworkSession(NetworkSessionStaticProvider staticprovider, SteamWorks steamAPI)
		{
			SteamNetworkSessionProvider steamNetworkSessionProvider = new SteamNetworkSessionProvider(staticprovider, steamAPI);
			return steamNetworkSessionProvider._networkSession = new NetworkSession(steamNetworkSessionProvider);
		}

		protected SteamNetworkSessionProvider(NetworkSessionStaticProvider staticProvider, SteamWorks steamAPI)
			: base(staticProvider)
		{
			_steamAPI = steamAPI;
		}

		public override void StartHost(NetworkSessionStaticProvider.BeginCreateSessionState sqs)
		{
			if (sqs.SessionType != 0)
			{
				CreateSessionInfo createSessionInfo = new CreateSessionInfo();
				createSessionInfo.SessionProperties = sqs.Properties;
				createSessionInfo.MaxPlayers = sqs.MaxPlayers;
				createSessionInfo.Name = sqs.ServerMessage;
				createSessionInfo.PasswordProtected = !string.IsNullOrEmpty(sqs.Password);
				createSessionInfo.JoinGamePolicy = JoinGamePolicy.Anyone;
				_steamAPI.CreateLobby(createSessionInfo, OnLobbyCreated, sqs);
			}
			else
			{
				HostSessionInfo hostSessionInfo = new HostSessionInfo();
				hostSessionInfo.JoinGamePolicy = JoinGamePolicy.InviteOnly;
				hostSessionInfo.Name = sqs.ServerMessage;
				hostSessionInfo.SessionProperties = sqs.Properties;
				hostSessionInfo.PasswordProtected = false;
				OnLobbyCreated(hostSessionInfo, sqs);
			}
		}

		protected void OnLobbyCreated(HostSessionInfo hostInfo, object context)
		{
			NetworkSessionStaticProvider.BeginCreateSessionState beginCreateSessionState = (NetworkSessionStaticProvider.BeginCreateSessionState)context;
			HostSessionInfo = hostInfo;
			_isHost = true;
			_sessionID = MathTools.RandomInt();
			_sessionType = beginCreateSessionState.SessionType;
			_maxPlayers = beginCreateSessionState.MaxPlayers;
			_signedInGamers = new List<SignedInGamer>(beginCreateSessionState.LocalGamers);
			_gameName = beginCreateSessionState.NetworkGameName;
			_properties = beginCreateSessionState.Properties;
			_version = beginCreateSessionState.Version;
			if (!string.IsNullOrWhiteSpace(_password))
			{
				_password = beginCreateSessionState.Password;
			}
			if (hostInfo == null)
			{
				beginCreateSessionState.ExceptionEncountered = new Exception("Could not create steam lobby");
				_hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
				_hostConnectionResultString = "Could not create steam lobby";
			}
			else
			{
				_hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				AddLocalGamer(_signedInGamers[0], true, 0, _steamAPI.SteamPlayerID);
			}
			beginCreateSessionState.Event.Set();
			if (beginCreateSessionState.Callback != null)
			{
				beginCreateSessionState.Callback(beginCreateSessionState);
			}
		}

		public override void StartClientInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState sqs, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			_hostConnectionResult = NetworkSession.ResultCode.Pending;
			KeeperOfTheInvitedGameData @object = new KeeperOfTheInvitedGameData(getPasswordCallback, null, lobbyId, sqs);
			_steamAPI.GetInvitedGameInfo(lobbyId, SessionUpdatedCallback, @object);
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as KeeperOfTheInvitedGameData;
			keeperOfTheInvitedGameData.SessionInfo = session;
			switch (updateresult)
			{
			case GameUpdateResultCode.Success:
				keeperOfTheInvitedGameData.State.AvailableSession = new AvailableNetworkSession(session);
				TaskDispatcher.Instance.AddTaskForMainThread(ValidateInvitedGameAndStartJoining, context);
				break;
			case GameUpdateResultCode.UnknownGame:
				_hostConnectionResultString = "Game Not Found";
				_hostConnectionResult = NetworkSession.ResultCode.Timeout;
				break;
			case GameUpdateResultCode.NoLongerValid:
				_hostConnectionResultString = "Steam no longer running";
				_hostConnectionResult = NetworkSession.ResultCode.Timeout;
				break;
			}
		}

		private void ValidateInvitedGameAndStartJoining(BaseTask task, object context)
		{
			KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as KeeperOfTheInvitedGameData;
			keeperOfTheInvitedGameData.Callback(keeperOfTheInvitedGameData.SessionInfo, keeperOfTheInvitedGameData, GotSessionPasswordCallback);
		}

		private void GotSessionPasswordCallback(bool cancelled, string password, string errorString, object context)
		{
			KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as KeeperOfTheInvitedGameData;
			if (cancelled)
			{
				_hostConnectionResultString = errorString;
				_hostConnectionResult = NetworkSession.ResultCode.UnknownResult;
			}
			else
			{
				keeperOfTheInvitedGameData.State.Password = password;
				StartClient(keeperOfTheInvitedGameData.State);
			}
		}

		public override void StartClient(NetworkSessionStaticProvider.BeginJoinSessionState sqs)
		{
			_isHost = false;
			_sessionType = sqs.SessionType;
			_sessionID = sqs.AvailableSession.SessionID;
			_properties = sqs.AvailableSession.SessionProperties;
			_maxPlayers = sqs.AvailableSession.MaxGamerCount;
			_signedInGamers = new List<SignedInGamer>(sqs.LocalGamers);
			_gameName = sqs.NetworkGameName;
			_version = sqs.Version;
			_hostConnectionResult = NetworkSession.ResultCode.Pending;
			if (_sessionType != 0)
			{
				RequestConnectToHostMessage requestConnectToHostMessage = new RequestConnectToHostMessage();
				requestConnectToHostMessage.SessionID = _sessionID;
				requestConnectToHostMessage.SessionProperties = _properties;
				requestConnectToHostMessage.Password = sqs.Password;
				requestConnectToHostMessage.Gamer = _signedInGamers[0];
				SteamNetBuffer steamNetBuffer = _steamAPI.AllocSteamNetBuffer();
				steamNetBuffer.Write(requestConnectToHostMessage, _gameName, _version);
				_steamAPI.JoinGame(sqs.AvailableSession.LobbySteamID, sqs.AvailableSession.HostSteamID, steamNetBuffer);
			}
		}

		private void SendRemoteData(SteamNetBuffer msg, NetDeliveryMethod flags, NetworkGamer recipient)
		{
			ulong alternateAddress = recipient.AlternateAddress;
			if (alternateAddress != 0)
			{
				_steamAPI.SendPacket(msg, alternateAddress, flags, 0);
			}
		}

		private NetDeliveryMethod GetDeliveryMethodFromOptions(SendDataOptions options)
		{
			NetDeliveryMethod result = NetDeliveryMethod.Unknown;
			switch (options)
			{
			case SendDataOptions.None:
				result = NetDeliveryMethod.Unreliable;
				break;
			case SendDataOptions.Reliable:
				result = NetDeliveryMethod.ReliableUnordered;
				break;
			case SendDataOptions.InOrder:
				result = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				result = NetDeliveryMethod.ReliableOrdered;
				break;
			}
			return result;
		}

		private void PrepareMessageForSending(SendDataOptions options, NetworkGamer recipient, out SteamNetBuffer msg, out int channel, out ulong netConnection, out NetDeliveryMethod flags)
		{
			if (recipient.NetProxyObject)
			{
				msg = _steamAPI.AllocSteamNetBuffer();
				flags = GetDeliveryMethodFromOptions(options);
				channel = 1;
				netConnection = _host.AlternateAddress;
				msg.Write((byte)3);
				msg.Write(recipient.Id);
				msg.Write((byte)flags);
				msg.Write(_localPlayerGID);
				if (flags == NetDeliveryMethod.ReliableUnordered)
				{
					flags = NetDeliveryMethod.ReliableOrdered;
				}
				return;
			}
			ulong alternateAddress = recipient.AlternateAddress;
			if (alternateAddress != 0)
			{
				msg = _steamAPI.AllocSteamNetBuffer();
				flags = GetDeliveryMethodFromOptions(options);
				msg.Write(recipient.Id);
				msg.Write(_localPlayerGID);
				channel = 0;
				netConnection = alternateAddress;
			}
			else
			{
				msg = null;
				channel = 0;
				flags = NetDeliveryMethod.Unknown;
				netConnection = 0uL;
			}
		}

		public override void SendRemoteData(byte[] data, SendDataOptions options, NetworkGamer recipient)
		{
			SteamNetBuffer msg;
			int channel;
			ulong netConnection;
			NetDeliveryMethod flags;
			PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
			if (netConnection != 0)
			{
				msg.WriteArray(data);
				_steamAPI.SendPacket(msg, netConnection, flags, channel);
			}
		}

		public override void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			SteamNetBuffer msg;
			int channel;
			ulong netConnection;
			NetDeliveryMethod flags;
			PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
			if (netConnection != 0)
			{
				msg.WriteArray(data, offset, length);
				_steamAPI.SendPacket(msg, netConnection, flags, channel);
			}
		}

		private void PrepareBroadcastMessageForSending(SendDataOptions options, out SteamNetBuffer msg, out NetDeliveryMethod flags)
		{
			msg = _steamAPI.AllocSteamNetBuffer();
			flags = GetDeliveryMethodFromOptions(options);
			msg.Write((byte)4);
			msg.Write((byte)flags);
			msg.Write(_localPlayerGID);
			if (flags == NetDeliveryMethod.ReliableUnordered)
			{
				flags = NetDeliveryMethod.ReliableOrdered;
			}
		}

		public override void BroadcastRemoteData(byte[] data, SendDataOptions options)
		{
			ulong alternateAddress = _host.AlternateAddress;
			if (alternateAddress != 0)
			{
				SteamNetBuffer msg;
				NetDeliveryMethod flags;
				PrepareBroadcastMessageForSending(options, out msg, out flags);
				msg.WriteArray(data);
				_steamAPI.SendPacket(msg, alternateAddress, flags, 1);
			}
		}

		public override void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options)
		{
			ulong alternateAddress = _host.AlternateAddress;
			if (alternateAddress != 0)
			{
				SteamNetBuffer msg;
				NetDeliveryMethod flags;
				PrepareBroadcastMessageForSending(options, out msg, out flags);
				msg.WriteArray(data, offset, length);
				_steamAPI.SendPacket(msg, alternateAddress, flags, 1);
			}
		}

		private bool HandleHostStatusChangedMessage(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.ReadByte())
			{
			case 5:
			{
				ConnectedMessage connectedMessage = new ConnectedMessage();
				byte b;
				do
				{
					if (_nextPlayerGID == 0)
					{
						_nextPlayerGID = 1;
					}
					b = _nextPlayerGID++;
				}
				while (_idToGamer.ContainsKey(b));
				connectedMessage.PlayerGID = b;
				connectedMessage.SetPeerList(_allGamers);
				SteamNetBuffer steamNetBuffer2 = _steamAPI.AllocSteamNetBuffer();
				steamNetBuffer2.Write((byte)1);
				steamNetBuffer2.Write(connectedMessage);
				_steamAPI.SendPacket(steamNetBuffer2, msg.SenderId, NetDeliveryMethod.ReliableOrdered, 1);
				NetworkGamer networkGamer2 = AddRemoteGamer(_steamIDToGamer[msg.SenderId], msg.SenderId, false, connectedMessage.PlayerGID);
				_steamIDToGamer[msg.SenderId] = networkGamer2;
				{
					foreach (NetworkGamer remoteGamer in _remoteGamers)
					{
						if (remoteGamer.AlternateAddress != msg.SenderId)
						{
							steamNetBuffer2 = _steamAPI.AllocSteamNetBuffer();
							steamNetBuffer2.Write((byte)0);
							steamNetBuffer2.Write(networkGamer2.Id);
							steamNetBuffer2.Write(networkGamer2);
							_steamAPI.SendPacket(steamNetBuffer2, remoteGamer.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
						}
					}
					return result;
				}
			}
			case 7:
				if (_steamIDToGamer.ContainsKey(msg.SenderId))
				{
					NetworkGamer networkGamer = _steamIDToGamer[msg.SenderId] as NetworkGamer;
					if (networkGamer != null)
					{
						DropPeerMessage dropPeerMessage = new DropPeerMessage();
						dropPeerMessage.PlayerGID = networkGamer.Id;
						foreach (NetworkGamer remoteGamer2 in _remoteGamers)
						{
							if (remoteGamer2.AlternateAddress != msg.SenderId)
							{
								SteamNetBuffer steamNetBuffer = _steamAPI.AllocSteamNetBuffer();
								steamNetBuffer.Write((byte)2);
								steamNetBuffer.Write(dropPeerMessage);
								_steamAPI.SendPacket(steamNetBuffer, remoteGamer2.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
							}
						}
						_steamIDToGamer.Remove(msg.SenderId);
						RemoveGamer(networkGamer);
					}
				}
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		private bool HandleHostSystemMessages(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.ReadByte())
			{
			case 3:
			{
				byte b2 = msg.ReadByte();
				NetDeliveryMethod deliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				NetworkGamer networkGamer = FindGamerById(b2);
				if (networkGamer != null)
				{
					byte b = msg.ReadByte();
					SteamNetBuffer steamNetBuffer2 = _steamAPI.AllocSteamNetBuffer();
					steamNetBuffer2.Write(b2);
					steamNetBuffer2.Write(b);
					steamNetBuffer2.CopyByteArrayFrom(msg);
					_steamAPI.SendPacket(steamNetBuffer2, networkGamer.AlternateAddress, deliveryMethod, 0);
				}
				break;
			}
			case 4:
			{
				NetDeliveryMethod deliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				byte b = msg.ReadByte();
				byte[] data = null;
				int num = msg.ReadInt32();
				int offset = 0;
				bool flag = false;
				if (num > 0)
				{
					flag = msg.GetAlignedData(out data, out offset);
					if (!flag)
					{
						data = msg.ReadBytes(num);
					}
				}
				LocalNetworkGamer localNetworkGamer = FindGamerById(0) as LocalNetworkGamer;
				if (localNetworkGamer == null)
				{
					break;
				}
				NetworkGamer sender = FindGamerById(b);
				if (flag)
				{
					localNetworkGamer.AppendNewDataPacket(data, offset, num, sender);
				}
				else
				{
					localNetworkGamer.AppendNewDataPacket(data, sender);
				}
				for (int i = 0; i < _remoteGamers.Count; i++)
				{
					if (_remoteGamers[i].Id == b)
					{
						continue;
					}
					ulong alternateAddress = _remoteGamers[i].AlternateAddress;
					if (alternateAddress != 0)
					{
						SteamNetBuffer steamNetBuffer = _steamAPI.AllocSteamNetBuffer();
						steamNetBuffer.Write(_remoteGamers[i].Id);
						steamNetBuffer.Write(b);
						steamNetBuffer.Write(num);
						if (num > 0)
						{
							steamNetBuffer.Write(data, offset, num);
						}
						_steamAPI.SendPacket(steamNetBuffer, alternateAddress, deliveryMethod, 0);
					}
				}
				break;
			}
			}
			return result;
		}

		private void HandleHostConnectionApproval(SteamNetBuffer msg)
		{
			RequestConnectToHostMessage requestConnectToHostMessage = msg.ReadRequestConnectToHostMessage(_gameName, _version);
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.GameNameInvalid)
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.GameNamesDontMatch);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.VersionInvalid)
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsLower)
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher)
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasNewerVersion);
				return;
			}
			if (!string.IsNullOrWhiteSpace(_password) && (string.IsNullOrWhiteSpace(requestConnectToHostMessage.Password) || !requestConnectToHostMessage.Password.Equals(_password)))
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.IncorrectPassword);
				return;
			}
			if (AllowConnectionCallbackAlt != null && !AllowConnectionCallbackAlt(requestConnectToHostMessage.Gamer.PlayerID, msg.SenderId))
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.ConnectionDenied);
				return;
			}
			if (requestConnectToHostMessage.SessionProperties.Count != _properties.Count)
			{
				FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
				return;
			}
			for (int i = 0; i < requestConnectToHostMessage.SessionProperties.Count; i++)
			{
				if (_properties[i].HasValue && requestConnectToHostMessage.SessionProperties[i] != _properties[i])
				{
					FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
					return;
				}
			}
			GamerCollection<NetworkGamer> allGamers = base.AllGamers;
			for (int j = 0; j < allGamers.Count; j++)
			{
				bool flag = false;
				if (allGamers[j] == null)
				{
					flag = true;
				}
				else if (allGamers[j].AlternateAddress == msg.SenderId)
				{
					flag = true;
				}
				else if (allGamers[j].Gamertag == requestConnectToHostMessage.Gamer.Gamertag)
				{
					flag = true;
				}
				if (flag)
				{
					FailConnection(msg.SenderId, NetworkSession.ResultCode.GamerAlreadyConnected);
					return;
				}
			}
			_steamIDToGamer[msg.SenderId] = requestConnectToHostMessage.Gamer;
			_steamAPI.AcceptConnection(msg.SenderId);
		}

		private bool HandleHostMessages(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.MessageType)
			{
			case NetIncomingMessageType.ConnectionApproval:
				HandleHostConnectionApproval(msg);
				break;
			case NetIncomingMessageType.StatusChanged:
				return HandleHostStatusChangedMessage(msg);
			case NetIncomingMessageType.Data:
				if (msg.Channel == 1)
				{
					return HandleHostSystemMessages(msg);
				}
				result = false;
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		private void AddNewPeer(SteamNetBuffer msg)
		{
			byte globalID = msg.ReadByte();
			Gamer gmr = msg.ReadGamer();
			AddProxyGamer(gmr, false, globalID);
		}

		private bool HandleClientSystemMessages(SteamNetBuffer msg)
		{
			bool result = true;
			InternalMessageTypes internalMessageTypes = (InternalMessageTypes)msg.ReadByte();
			NetworkGamer value = null;
			switch (internalMessageTypes)
			{
			case InternalMessageTypes.NewPeer:
				AddNewPeer(msg);
				break;
			case InternalMessageTypes.ResponseToConnection:
			{
				ConnectedMessage connectedMessage = msg.ReadConnectedMessage();
				AddLocalGamer(_signedInGamers[0], false, connectedMessage.PlayerGID, _steamAPI.SteamPlayerID);
				for (int j = 0; j < connectedMessage.Peers.Length; j++)
				{
					if (connectedMessage.ids[j] == 0)
					{
						AddRemoteGamer(connectedMessage.Peers[j], msg.SenderId, true, 0);
					}
					else
					{
						AddProxyGamer(connectedMessage.Peers[j], false, connectedMessage.ids[j]);
					}
				}
				break;
			}
			case InternalMessageTypes.DropPeer:
			{
				DropPeerMessage dropPeerMessage = msg.ReadDropPeerMessage();
				if (_idToGamer.TryGetValue(dropPeerMessage.PlayerGID, out value))
				{
					RemoveGamer(value);
				}
				break;
			}
			case InternalMessageTypes.SessionPropertiesChanged:
			{
				NetworkSessionProperties networkSessionProperties = msg.ReadSessionProps();
				for (int i = 0; i < networkSessionProperties.Count; i++)
				{
					if (networkSessionProperties[i].HasValue && _properties[i].HasValue)
					{
						_properties[i] = networkSessionProperties[i];
					}
				}
				break;
			}
			default:
				result = false;
				break;
			}
			return result;
		}

		private void HandleClientStatusChangedMessage(SteamNetBuffer msg)
		{
			switch (msg.ReadByte())
			{
			case 6:
				break;
			case 5:
				_hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				_hostConnectionResultString = _hostConnectionResult.ToString();
				break;
			case 7:
				HandleDisconnection(msg.ReadString());
				break;
			}
		}

		private bool HandleClientMessages(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.MessageType)
			{
			case NetIncomingMessageType.StatusChanged:
				HandleClientStatusChangedMessage(msg);
				break;
			case NetIncomingMessageType.Data:
				if (msg.Channel == 1)
				{
					return HandleClientSystemMessages(msg);
				}
				result = false;
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		private void FailConnection(ulong c, NetworkSession.ResultCode reason)
		{
			_steamAPI.Deny(c, reason.ToString());
		}

		private bool HandleCommonMessages(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.MessageType)
			{
			case NetIncomingMessageType.Data:
			{
				byte gamerId = msg.ReadByte();
				NetworkGamer networkGamer = FindGamerById(gamerId);
				if (networkGamer == null)
				{
					break;
				}
				LocalNetworkGamer localNetworkGamer = networkGamer as LocalNetworkGamer;
				if (localNetworkGamer != null)
				{
					byte gamerId2 = msg.ReadByte();
					NetworkGamer networkGamer2 = FindGamerById(gamerId2);
					if (networkGamer2 != null)
					{
						byte[] data = msg.ReadByteArray();
						localNetworkGamer.AppendNewDataPacket(data, networkGamer2);
					}
				}
				break;
			}
			default:
				result = false;
				break;
			case NetIncomingMessageType.VerboseDebugMessage:
			case NetIncomingMessageType.DebugMessage:
			case NetIncomingMessageType.WarningMessage:
			case NetIncomingMessageType.ErrorMessage:
				break;
			}
			return result;
		}

		public override void Update()
		{
			if (!_steamAPI.Update())
			{
				return;
			}
			SteamNetBuffer packet;
			while ((packet = _steamAPI.GetPacket()) != null)
			{
				if (!(_isHost ? HandleHostMessages(packet) : HandleClientMessages(packet)))
				{
					bool flag = HandleCommonMessages(packet);
				}
				_steamAPI.FreeSteamNetBuffer(packet);
				packet = null;
			}
		}

		public override void Dispose(bool disposeManagedObjects)
		{
			_staticProvider.TaskScheduler.Exit();
			if (_steamAPI.InSession)
			{
				_steamAPI.LeaveSession();
			}
			base.Dispose(disposeManagedObjects);
		}

		public override void ReportClientJoined(string username)
		{
		}

		public override void ReportClientLeft(string username)
		{
		}

		public override void ReportSessionAlive()
		{
		}

		public override void UpdateHostSession(string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
		{
			if (!string.IsNullOrWhiteSpace(serverName))
			{
				HostSessionInfo.Name = serverName;
			}
			if (passwordProtected.HasValue)
			{
				HostSessionInfo.PasswordProtected = passwordProtected.Value;
			}
			if (sessionProps != null)
			{
				HostSessionInfo.SessionProperties = sessionProps;
			}
			_steamAPI.UpdateHostLobbyData(HostSessionInfo);
		}

		public override void UpdateHostSessionJoinPolicy(JoinGamePolicy joinGamePolicy)
		{
			HostSessionInfo.JoinGamePolicy = joinGamePolicy;
			_steamAPI.UpdateHostLobbyData(HostSessionInfo);
		}

		public override void CloseNetworkSession()
		{
		}
	}
}
