using DNA.CastleMinerZ.Net;
using System;

namespace DNA.CastleMinerZ.AI
{
	public class FelguardGiveUp : ZombieGiveUp
	{
		public override void Enter(BaseZombie entity)
		{
			ZeroVelocity(entity);
			entity.CurrentPlayer = entity.PlayClip("Idle", false, TimeSpan.FromSeconds(0.25));
			if (entity.Target != null && entity.Target.IsLocal && entity.Target.Gamer != null)
			{
				EnemyGiveUpMessage.Send(entity.EnemyID, entity.Target.Gamer.Id);
			}
		}
	}
}
