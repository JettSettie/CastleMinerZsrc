using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;
using System;
using System.Diagnostics;

namespace DNA.CastleMinerZ.Net
{
	public class ServerInfo
	{
		private const float _sRetryInterval = 2f;

		public AvailableNetworkSession Session;

		private string _ipAddressString;

		private string _serverName = "";

		private string _errorMessage = Strings.Waiting_for_server_to_respond____;

		private string _previousMessage = "";

		private int _numPlayers;

		private string _numPlayersString = "0";

		private int _maxPlayers;

		private string _maxPlayersString = "0";

		private bool _isOnline = true;

		private bool _passwordProtected;

		private string _hostUsername = "";

		private string _dateCreated;

		private CastleMinerZGame.PVPEnum _pvp;

		private bool _isValidIP = true;

		private bool _refreshing;

		private HostDiscovery.ResultCode _nextResult;

		private HostDiscovery.ResultCode _currentResult = HostDiscovery.ResultCode.TimedOut;

		private GameModeTypes _gameMode;

		private GameDifficultyTypes _gameDifficulty;

		private bool _infiniteResourceMode;

		private int _requestID = -1;

		private Stopwatch _updateTimer;

		private int _numFriends;

		private string _numFriendsStr;

		private int _proximity;

		public bool IsValidIP
		{
			get
			{
				return _isValidIP;
			}
		}

		public bool WaitingForResponseFromHost
		{
			get
			{
				return _nextResult == HostDiscovery.ResultCode.Pending;
			}
		}

		public bool PasswordProtected
		{
			get
			{
				return _passwordProtected;
			}
		}

		public bool ReadyToJoin
		{
			get
			{
				if (_currentResult == HostDiscovery.ResultCode.Success)
				{
					return Session != null;
				}
				return false;
			}
		}

		public GameDifficultyTypes GameDifficulty
		{
			get
			{
				return _gameDifficulty;
			}
		}

		public string DateCreatedStr
		{
			get
			{
				return _dateCreated;
			}
		}

		public DateTime DateCreated
		{
			get
			{
				if (Session != null)
				{
					return Session.DateCreated;
				}
				return DateTime.MinValue;
			}
		}

		public string GameDifficultyString
		{
			get
			{
				switch (_gameDifficulty)
				{
				case GameDifficultyTypes.EASY:
					return Strings.Easy;
				case GameDifficultyTypes.HARD:
					return Strings.Hard;
				case GameDifficultyTypes.HARDCORE:
					return Strings.Hardcore;
				case GameDifficultyTypes.NOENEMIES:
					return Strings.No_Enemies;
				default:
					return "";
				}
			}
		}

		public GameModeTypes GameMode
		{
			get
			{
				return _gameMode;
			}
		}

		public string GameModeString
		{
			get
			{
				switch (_gameMode)
				{
				default:
					if (_infiniteResourceMode)
					{
						return Strings.Creative;
					}
					return "";
				case GameModeTypes.Creative:
					return Strings.Creative;
				case GameModeTypes.DragonEndurance:
					return Strings.Dragon_Endurance;
				case GameModeTypes.Endurance:
					return Strings.Endurance;
				case GameModeTypes.Survival:
					return Strings.Survival;
				case GameModeTypes.Scavenger:
					return Strings.Scavenger;
				case GameModeTypes.Exploration:
					return Strings.Exploration;
				}
			}
		}

		public string HostUsername
		{
			get
			{
				return _hostUsername;
			}
			set
			{
				_hostUsername = value;
			}
		}

		public string ServerName
		{
			get
			{
				switch (_currentResult)
				{
				case HostDiscovery.ResultCode.Pending:
				case HostDiscovery.ResultCode.Success:
					return _serverName;
				default:
					return _errorMessage;
				}
			}
		}

		public string IPAddressString
		{
			get
			{
				return _ipAddressString;
			}
		}

		public int NumberPlayers
		{
			get
			{
				return _numPlayers;
			}
		}

		public string NumberPlayerString
		{
			get
			{
				return _numPlayersString;
			}
		}

		public int MaxPlayers
		{
			get
			{
				return _maxPlayers;
			}
		}

		public string MaxPlayersString
		{
			get
			{
				return _maxPlayersString;
			}
		}

		public bool IsOnline
		{
			get
			{
				return _isOnline;
			}
		}

		public string PVPstr
		{
			get
			{
				switch (_pvp)
				{
				case CastleMinerZGame.PVPEnum.Everyone:
					return Strings.Everyone;
				case CastleMinerZGame.PVPEnum.NotFriends:
					return Strings.Non_Friends_Only;
				case CastleMinerZGame.PVPEnum.Off:
					return Strings.Off;
				default:
					return "";
				}
			}
		}

		public int NumFriends
		{
			get
			{
				return _numFriends;
			}
		}

		public string NumFriendsStr
		{
			get
			{
				return _numFriendsStr;
			}
		}

		public int Proximity
		{
			get
			{
				return _proximity;
			}
		}

		public ServerInfo(AvailableNetworkSession session)
		{
			if (session.IPEndPoint == null)
			{
				_ipAddressString = "No ip address";
			}
			else
			{
				_ipAddressString = session.IPEndPoint.Address.ToString();
			}
			_isValidIP = true;
			_isOnline = true;
			Session = session;
			_serverName = session.ServerMessage;
			_hostUsername = session.HostGamertag;
			_numPlayers = session.CurrentGamerCount;
			_numPlayersString = _numPlayers.ToString();
			_maxPlayers = session.MaxGamerCount;
			_maxPlayersString = _maxPlayers.ToString();
			_passwordProtected = session.PasswordProtected;
			_numFriends = session.FriendCount;
			_numFriendsStr = _numFriends.ToString();
			_proximity = session.Proximity;
			_gameMode = (GameModeTypes)session.SessionProperties[2].Value;
			_gameDifficulty = (GameDifficultyTypes)session.SessionProperties[3].Value;
			_infiniteResourceMode = (session.SessionProperties[4] == 1);
			if (session.SessionProperties[5].HasValue)
			{
				_pvp = (CastleMinerZGame.PVPEnum)session.SessionProperties[5].Value;
			}
			else
			{
				_pvp = CastleMinerZGame.PVPEnum.Off;
			}
		}

		public void DiscoveryCallback(HostDiscovery.ResultCode result, AvailableNetworkSession session, object context)
		{
			_nextResult = result;
			_currentResult = result;
			_requestID = -1;
			_refreshing = false;
			switch (result)
			{
			case HostDiscovery.ResultCode.GamerAlreadyConnected:
				break;
			case HostDiscovery.ResultCode.Success:
				_isValidIP = true;
				_isOnline = true;
				Session = session;
				_serverName = session.ServerMessage;
				_hostUsername = session.HostGamertag;
				_numPlayers = session.CurrentGamerCount;
				_numPlayersString = _numPlayers.ToString();
				_maxPlayers = session.MaxGamerCount;
				_maxPlayersString = _maxPlayers.ToString();
				_passwordProtected = session.PasswordProtected;
				_gameMode = (GameModeTypes)session.SessionProperties[2].Value;
				_gameDifficulty = (GameDifficultyTypes)session.SessionProperties[3].Value;
				_infiniteResourceMode = (session.SessionProperties[4] == 1);
				if (session.SessionProperties[5].HasValue)
				{
					_pvp = (CastleMinerZGame.PVPEnum)session.SessionProperties[5].Value;
				}
				else
				{
					_pvp = CastleMinerZGame.PVPEnum.Off;
				}
				break;
			case HostDiscovery.ResultCode.TimedOut:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.Waiting_for_server_to_respond____;
				break;
			case HostDiscovery.ResultCode.HostNameInvalid:
				_isValidIP = false;
				_isOnline = false;
				_errorMessage = Strings.Host_name_is_not_valid;
				break;
			case HostDiscovery.ResultCode.FailedToResolveHostName:
				_isValidIP = false;
				_isOnline = false;
				_errorMessage = Strings.Could_not_resolve_host_name;
				break;
			case HostDiscovery.ResultCode.ServerHasNewerVersion:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.Version_mismatch__Server_is_newer;
				break;
			case HostDiscovery.ResultCode.ServerHasOlderVersion:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.Version_mismatch__Server_is_older;
				break;
			case HostDiscovery.ResultCode.VersionIsInvalid:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.Version_number_was_invalid;
				break;
			case HostDiscovery.ResultCode.ConnectionDenied:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.Connection_was_denied_by_server;
				break;
			case HostDiscovery.ResultCode.WrongGameName:
				_isValidIP = true;
				_isOnline = false;
				_errorMessage = Strings.This_is_not_a_CastleMiner_Z_server;
				break;
			}
		}

		public void RefreshServerStatus(HostDiscovery discovery)
		{
			if ((_updateTimer == null || _updateTimer.Elapsed.TotalSeconds > 2.0) && !WaitingForResponseFromHost && (_currentResult == HostDiscovery.ResultCode.Success || _currentResult == HostDiscovery.ResultCode.TimedOut))
			{
				UpdateServerStatus(discovery);
			}
		}

		public void UpdateServerStatus(HostDiscovery discovery)
		{
			_updateTimer = Stopwatch.StartNew();
			if (WaitingForResponseFromHost)
			{
				discovery.RemovePendingRequest(_requestID);
			}
			_nextResult = HostDiscovery.ResultCode.Pending;
			_requestID = discovery.GetHostInfo(Session.LobbySteamID, DiscoveryCallback, null);
		}
	}
}
