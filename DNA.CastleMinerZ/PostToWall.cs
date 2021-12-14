using DNA.CastleMinerZ.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace DNA.CastleMinerZ
{
	public class PostToWall
	{
		public string Message = "";

		public string AccessToken = "";

		public string Link = "";

		public string Caption = "";

		public string Description = "";

		public string ImageURL = "http://digitaldnagames.com/Images/DNABanner.png";

		public string ActionName = Strings.Create_Account;

		public string ActionURL = "http://digitaldnagames.com/Account/FacebookRegister.aspx";

		public string ErrorMessage
		{
			get;
			private set;
		}

		public string PostID
		{
			get;
			private set;
		}

		private string AppendQueryString(string query, string data)
		{
			return "&" + query + "=" + data;
		}

		public void Post()
		{
			if (!string.IsNullOrEmpty(Message))
			{
				string requestUriString = "https://graph.facebook.com/me/feed?access_token=" + AccessToken;
				string s = "?name=name" + AppendQueryString("link", Link) + AppendQueryString("caption", Caption) + AppendQueryString("description", Description) + AppendQueryString("source", ImageURL) + AppendQueryString("actions", "{\"name\": \"" + ActionName + "\", \"link\": \"" + ActionURL + "\"}") + AppendQueryString("message", Message);
				WebRequest webRequest = WebRequest.Create(requestUriString);
				webRequest.ContentType = "application/x-www-form-urlencoded";
				webRequest.Method = "POST";
				byte[] bytes = Encoding.ASCII.GetBytes(s);
				webRequest.ContentLength = bytes.Length;
				Stream requestStream = webRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
				try
				{
					WebResponse response = webRequest.GetResponse();
					StreamReader streamReader = null;
					try
					{
						streamReader = new StreamReader(response.GetResponseStream());
						PostID = streamReader.ReadToEnd();
					}
					finally
					{
						if (streamReader != null)
						{
							streamReader.Close();
						}
					}
				}
				catch (WebException ex)
				{
					StreamReader streamReader2 = null;
					try
					{
						streamReader2 = new StreamReader(ex.Response.GetResponseStream());
						ErrorMessage = streamReader2.ReadToEnd();
					}
					finally
					{
						if (streamReader2 != null)
						{
							streamReader2.Close();
						}
					}
				}
			}
		}
	}
}
