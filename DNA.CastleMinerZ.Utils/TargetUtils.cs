using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Utils
{
	public class TargetUtils
	{
		public const float c_playerHeightInBlocks = 1.5f;

		private const float MAX_VIEW_DOT = 0.17f;

		protected static TraceProbe tp = new TraceProbe();

		public static TargetSearchResult FindBestTarget(DragonEntity entity, Vector3 forward, float maxViewDistance)
		{
			float num = float.MaxValue;
			bool flag = true;
			TargetSearchResult result = default(TargetSearchResult);
			result.player = null;
			Vector3 worldPosition = entity.WorldPosition;
			worldPosition += entity.LocalToWorld.Forward * 7f;
			float num2 = maxViewDistance * maxViewDistance;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer == null)
					{
						continue;
					}
					Player player = (Player)networkGamer.Tag;
					if (player == null || !player.ValidLivingGamer)
					{
						continue;
					}
					Vector3 worldPosition2 = player.WorldPosition;
					worldPosition2.Y += 1.5f;
					Vector3 vector = worldPosition2 - worldPosition;
					float num3 = vector.LengthSquared();
					if (num3 < num2 && num3 > 0.001f)
					{
						flag = false;
						if (!BlockTerrain.Instance.RegionIsLoaded(worldPosition2))
						{
							continue;
						}
						float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(worldPosition2);
						if (!(simpleSunlightAtPoint > 0f))
						{
							continue;
						}
						float num4 = num3 / (simpleSunlightAtPoint * simpleSunlightAtPoint);
						if (!(num4 < num))
						{
							continue;
						}
						vector.Normalize();
						if (Vector3.Dot(vector, forward) > 0.17f)
						{
							tp.Init(worldPosition, worldPosition2);
							BlockTerrain.Instance.Trace(tp);
							if (!tp._collides)
							{
								result.player = player;
								result.distance = (float)Math.Sqrt(num3);
								result.light = simpleSunlightAtPoint;
								result.vectorToPlayer = vector;
								num = num4;
							}
						}
					}
					else if (num3 < MathTools.Square(entity.EType.SpawnDistance * 1.25f))
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				entity.Removed = true;
				EnemyManager.Instance.RemoveDragon();
			}
			return result;
		}

		public static List<TargetSearchResult> FindTargetsInRange(Vector3 centerPoint, float range, bool ignoreLighting = true)
		{
			List<TargetSearchResult> list = new List<TargetSearchResult>();
			float num = range * range;
			if (CastleMinerZGame.Instance.CurrentNetworkSession == null)
			{
				return list;
			}
			for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
				if (networkGamer == null)
				{
					continue;
				}
				Player player = (Player)networkGamer.Tag;
				if (player == null || !player.ValidLivingGamer)
				{
					continue;
				}
				Vector3 worldPosition = player.WorldPosition;
				worldPosition.Y += 1.5f;
				Vector3 vectorToPlayer = worldPosition - centerPoint;
				float num2 = vectorToPlayer.LengthSquared();
				if (!(num2 > num) && BlockTerrain.Instance.RegionIsLoaded(worldPosition))
				{
					float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(worldPosition);
					if (simpleSunlightAtPoint > 0f || ignoreLighting)
					{
						vectorToPlayer.Normalize();
						TargetSearchResult item = default(TargetSearchResult);
						item.player = player;
						item.distance = (float)Math.Sqrt(num2);
						item.light = simpleSunlightAtPoint;
						item.vectorToPlayer = vectorToPlayer;
						list.Add(item);
					}
				}
			}
			return list;
		}
	}
}
