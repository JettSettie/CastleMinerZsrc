using DNA.Drawing;
using DNA.Drawing.Imaging;
using DNA.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class ProfiledContentManager : ContentManager
	{
		private static readonly string[] _cImageExtensions = new string[6]
		{
			"*.dds",
			"*.tiff",
			"*.png",
			"*.jpg",
			"*.jpeg",
			"*.bmp"
		};

		private string _reachRoot;

		private string _hiDefRoot;

		private string _reachRootLower;

		private string _hiDefRootLower;

		private Stopwatch _loadingTimer;

		private bool _checkUserTextures;

		private string _userTexturesDefault;

		private Dictionary<string, string> _availableTextures;

		private Dictionary<string, Texture2D> _preLoadedTextures;

		private int TextureLevel;

		public ProfiledContentManager(IServiceProvider services, string reachRoot, string hiDefRoot, int textureLevel)
			: base(services)
		{
			TextureLevel = textureLevel;
			_reachRoot = reachRoot;
			_reachRootLower = reachRoot.ToLower();
			_hiDefRoot = hiDefRoot;
			_hiDefRootLower = hiDefRoot.ToLower();
			if (CastleMinerZArgs.Instance.TextureFolder != null)
			{
				_checkUserTextures = false;
				int startIndex = CastleMinerZArgs.Instance.TextureFolder.Length + 1;
				string[] cImageExtensions = _cImageExtensions;
				foreach (string pattern in cImageExtensions)
				{
					string[] array = PathTools.RecursivelyGetFiles(CastleMinerZArgs.Instance.TextureFolder, pattern);
					if (array == null || array.Length <= 0)
					{
						continue;
					}
					if (!_checkUserTextures)
					{
						_userTexturesDefault = CastleMinerZArgs.Instance.TextureFolder;
						_checkUserTextures = true;
						_availableTextures = new Dictionary<string, string>();
						_preLoadedTextures = new Dictionary<string, Texture2D>();
					}
					string[] array2 = array;
					foreach (string text in array2)
					{
						string text2 = Path.ChangeExtension(text, null);
						text2 = text2.Substring(startIndex);
						text2 = text2.ToLower();
						if (text2.EndsWith("_m"))
						{
							text2 = text2.Substring(0, text2.Length - 2);
						}
						if (!_availableTextures.ContainsKey(text2))
						{
							_availableTextures.Add(text2, text);
						}
					}
				}
			}
			else
			{
				_checkUserTextures = false;
			}
			_loadingTimer = Stopwatch.StartNew();
		}

		internal Texture[] LoadTerrain()
		{
			string path = Path.Combine(CastleMinerZArgs.Instance.TextureFolder, "terrain_" + (GraphicsProfileManager.Instance.IsHiDef ? "hidef.dat" : "reach.dat"));
			if (!File.Exists(path))
			{
				return null;
			}
			Texture[] array = null;
			using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReader binaryReader = new BinaryReader(input);
				array = new Texture[5];
				int num = GraphicsProfileManager.Instance.IsHiDef ? 2048 : 1024;
				uint[] array2 = new uint[num * num];
				byte[] array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				array3 = null;
				Texture2D texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData(array2);
				array[0] = texture2D;
				array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				array3 = null;
				texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData(array2);
				array[1] = texture2D;
				array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				array3 = null;
				texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData(array2);
				array[2] = texture2D;
				Texture2D texture2D2 = (Texture2D)(array[4] = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num / 2, num / 2, true, SurfaceFormat.Color));
				Texture2D texture2D3 = (Texture2D)(array[3] = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num / 2, num / 2, true, SurfaceFormat.Color));
				int num2 = num / 2;
				int num3 = 0;
				do
				{
					array2 = new uint[num2 * num2];
					array3 = binaryReader.ReadBytes(num2 * num2 * 4);
					Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
					array3 = null;
					texture2D2.SetData(num3, null, array2, 0, array2.Length);
					array3 = binaryReader.ReadBytes(num2 * num2 * 4);
					Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
					array3 = null;
					texture2D3.SetData(num3, null, array2, 0, array2.Length);
					num3++;
					num2 >>= 1;
				}
				while (num2 != 0);
				binaryReader.Close();
				return array;
			}
		}

		private Texture2D TryLoadFromFile(string assetName)
		{
			Texture2D value;
			if (_preLoadedTextures.TryGetValue(assetName, out value))
			{
				return value;
			}
			string text = assetName.ToLower();
			bool flag = false;
			string value2;
			flag = _availableTextures.TryGetValue(text, out value2);
			if (!flag)
			{
				flag = ((!GraphicsProfileManager.Instance.IsHiDef) ? _availableTextures.TryGetValue(Path.Combine(_reachRootLower, text), out value2) : _availableTextures.TryGetValue(Path.Combine(_hiDefRootLower, text), out value2));
			}
			if (!flag)
			{
				return null;
			}
			bool flag2 = false;
			text = value2.ToLower();
			bool flag3 = Path.ChangeExtension(text, null).EndsWith("_m");
			bool normalizeMipmaps = false;
			if (flag3)
			{
				normalizeMipmaps = (text.Contains("_nrm_") || text.Contains("_n_"));
			}
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDeviceTimed(ref _loadingTimer))
				{
					flag2 = true;
					try
					{
						value = TextureLoader.LoadFromFile(CastleMinerZGame.Instance.GraphicsDevice, value2, flag3, normalizeMipmaps);
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
				}
				if (!flag2)
				{
					Thread.Sleep(10);
				}
			}
			while (!flag2);
			_preLoadedTextures.Add(assetName, value);
			return value;
		}

		public static T Cast<T>(object v)
		{
			if (v == null)
			{
				return default(T);
			}
			return (T)v;
		}

		public override T Load<T>(string assetName)
		{
			Type typeFromHandle = typeof(T);
			bool flag = typeFromHandle.IsSubclassOf(typeof(Texture)) || typeFromHandle == typeof(Texture);
			T val = default(T);
			_loadingTimer.Restart();
			if (_checkUserTextures && flag)
			{
				val = Cast<T>(TryLoadFromFile(assetName));
				if (val != null)
				{
					return val;
				}
			}
			List<string> list = new List<string>();
			if (TextureLevel > 1 && flag)
			{
				string text = "_L" + TextureLevel;
				list.Add(assetName + text);
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					list.Add(_hiDefRoot + "\\" + assetName + text);
				}
				else
				{
					list.Add(_reachRoot + "\\" + assetName + text);
				}
			}
			list.Add(assetName);
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				list.Add(_hiDefRoot + "\\" + assetName);
			}
			else
			{
				list.Add(_reachRoot + "\\" + assetName);
			}
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					return base.Load<T>(list[i]);
				}
				catch
				{
				}
			}
			throw new Exception("Asset not found " + assetName);
		}
	}
}
