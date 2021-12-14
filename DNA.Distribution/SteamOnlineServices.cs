using DNA.Distribution.Steam;
using System;

namespace DNA.Distribution
{
	public class SteamOnlineServices : OnlineServices, IDisposable
	{
		public SteamWorks SteamAPI;

		private bool _disposed;

		public bool OperationWasSuccessful
		{
			get
			{
				if (SteamAPI != null)
				{
					return SteamAPI.OperationWasSuccessful;
				}
				return true;
			}
		}

		public SteamErrorCode ErrorCode
		{
			get
			{
				if (SteamAPI != null)
				{
					return SteamAPI.ErrorCode;
				}
				return SteamErrorCode.Disposed;
			}
		}

		public SteamOnlineServices(Guid productID, uint steamID)
			: base(productID)
		{
			SteamAPI = new SteamWorks(steamID);
			if (SteamAPI.OperationWasSuccessful)
			{
				_username = SteamAPI.SteamName;
				_steamUserID = SteamAPI.SteamPlayerID;
			}
		}

		public override DateTime GetServerTime()
		{
			return DateTime.UtcNow;
		}

		public override void Update(TimeSpan elapsedTime, TimeSpan totalTime)
		{
			if (!_disposed && SteamAPI != null)
			{
				SteamAPI.MinimalUpdate();
			}
		}

		public override bool ValidateLicense(string userName, string password, out string reason)
		{
			_username = userName;
			reason = "success";
			return true;
		}

		public override bool ValidateLicenseFacebook(string facebookID, string accessToken, out string username, out string reason)
		{
			reason = "facebookUser";
			username = "facebookUser";
			_username = reason;
			return true;
		}

		public override void AcceptTerms(string userName, string password)
		{
		}

		public override void AcceptTermsFacebook(string facebookID)
		{
		}

		public override bool RegisterFacebook(string facebookID, string accessToken, string email, string userName, string password, out string reason)
		{
			reason = "success";
			return true;
		}

		public override string GetLauncherPage()
		{
			return "http://www.castleminer.com";
		}

		public override string GetProductTitle()
		{
			return "Null Title";
		}

		public override int? GetAddOn(Guid guid)
		{
			return null;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing && SteamAPI != null)
				{
					SteamAPI.Unintialize();
				}
				SteamAPI = null;
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
