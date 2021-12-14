using DNA.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class GraphicsProfileManager
	{
		private static GraphicsProfileManager _instance;

		private GraphicsProfile _profile;

		public static GraphicsProfileManager Instance
		{
			get
			{
				return _instance;
			}
		}

		public GraphicsProfile Profile
		{
			get
			{
				return _profile;
			}
		}

		public bool IsHiDef
		{
			get
			{
				return Profile == GraphicsProfile.HiDef;
			}
		}

		public bool IsReach
		{
			get
			{
				return Profile == GraphicsProfile.Reach;
			}
		}

		static GraphicsProfileManager()
		{
			_instance = new GraphicsProfileManager();
		}

		public void ExamineGraphicsDevices(object sender, PreparingDeviceSettingsEventArgs args)
		{
			bool forceReachProfile = CommandLineArgs.Get<CastleMinerZArgs>().ForceReachProfile;
			GraphicsAdapter graphicsAdapter = args.GraphicsDeviceInformation.Adapter;
			if (graphicsAdapter == null)
			{
				graphicsAdapter = GraphicsAdapter.DefaultAdapter;
			}
			if (graphicsAdapter != null)
			{
				if (!forceReachProfile && graphicsAdapter.IsProfileSupported(GraphicsProfile.HiDef))
				{
					_profile = GraphicsProfile.HiDef;
				}
				else
				{
					_profile = GraphicsProfile.Reach;
				}
				args.GraphicsDeviceInformation.Adapter = graphicsAdapter;
				args.GraphicsDeviceInformation.GraphicsProfile = _profile;
				return;
			}
			if (!forceReachProfile)
			{
				for (int i = 0; i < GraphicsAdapter.Adapters.Count; i++)
				{
					if (GraphicsAdapter.Adapters[i].IsProfileSupported(GraphicsProfile.HiDef))
					{
						args.GraphicsDeviceInformation.Adapter = GraphicsAdapter.Adapters[i];
						args.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
						_profile = GraphicsProfile.HiDef;
						return;
					}
				}
			}
			int num = 0;
			while (true)
			{
				if (num < GraphicsAdapter.Adapters.Count)
				{
					if (GraphicsAdapter.Adapters[num].IsProfileSupported(GraphicsProfile.Reach))
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			args.GraphicsDeviceInformation.Adapter = GraphicsAdapter.Adapters[num];
			args.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.Reach;
			_profile = GraphicsProfile.Reach;
		}
	}
}
