using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.Drawing;
using DNA.IO;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace DNA.CastleMinerZ
{
	public class GlobalSettings
	{
		public bool FullScreen;

		public bool AskForFacebook = true;

		public Size ScreenSize = new Size(1280, 720);

		public int TextureQualityLevel = 1;

		public GlobalSettings()
		{
			if (GraphicsProfileManager.Instance.Profile == GraphicsProfile.Reach)
			{
				TextureQualityLevel = 2;
			}
		}

		public static string GetAppDataDirectory(string subdir)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string text = Path.Combine(folderPath, "CastleMinerZ");
			if (!string.IsNullOrEmpty(subdir))
			{
				text = Path.Combine(text, subdir);
			}
			return text;
		}

		public static string GetAppDataDirectory()
		{
			return GetAppDataDirectory(null);
		}

		public void Save()
		{
			HTFDocument hTFDocument = new HTFDocument();
			string appDataDirectory = GetAppDataDirectory();
			if (!Directory.Exists(appDataDirectory))
			{
				Directory.CreateDirectory(appDataDirectory);
			}
			string filename = Path.Combine(appDataDirectory, "user.settings");
			hTFDocument.Children.Add(new HTFElement("FullScreen", FullScreen.ToString()));
			hTFDocument.Children.Add(new HTFElement("AskForFacebook", AskForFacebook.ToString()));
			hTFDocument.Children.Add(new HTFElement("ScreenWidth", ScreenSize.Width.ToString()));
			hTFDocument.Children.Add(new HTFElement("ScreenHeight", ScreenSize.Height.ToString()));
			hTFDocument.Children.Add(new HTFElement("TextureQuality", TextureQualityLevel.ToString()));
			hTFDocument.Save(filename);
		}

		public void Load()
		{
			try
			{
				HTFDocument hTFDocument = new HTFDocument();
				string appDataDirectory = GetAppDataDirectory();
				string path = Path.Combine(appDataDirectory, "user.settings");
				hTFDocument.Load(path);
				for (int i = 0; i < hTFDocument.Children.Count; i++)
				{
					switch (hTFDocument.Children[i].ID)
					{
					case "FullScreen":
						FullScreen = bool.Parse(hTFDocument.Children[i].Value);
						break;
					case "AskForFacebook":
						AskForFacebook = bool.Parse(hTFDocument.Children[i].Value);
						break;
					case "ScreenWidth":
						ScreenSize.Width = int.Parse(hTFDocument.Children[i].Value);
						break;
					case "ScreenHeight":
						ScreenSize.Height = int.Parse(hTFDocument.Children[i].Value);
						break;
					case "TextureQuality":
						TextureQualityLevel = int.Parse(hTFDocument.Children[i].Value);
						break;
					}
				}
			}
			catch
			{
			}
		}
	}
}
