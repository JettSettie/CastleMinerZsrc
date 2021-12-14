namespace DNA.CastleMinerZ.AI
{
	public class FelguardEnemyType : EnemyType
	{
		public FelguardEnemyType(bool isBoss)
			: base(isBoss ? EnemyTypeEnum.HELL_LORD : EnemyTypeEnum.FELGUARD, isBoss ? ModelNameEnum.HELL_LORD : ModelNameEnum.FELGUARD, TextureNameEnum.FELGUARD, FoundInEnum.HELL, SessionStats.StatType.FelguardDefeated)
		{
			ChanceOfBulletStrike = 1f;
			SpawnRadius = 10;
			StartingDistanceLimit = 40;
		}

		public override float GetMaxSpeed()
		{
			return MathTools.RandomFloat(2f, 5.5f);
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.FelguardEmerge;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			return EnemyStates.FelguardTryAttack;
		}

		public override IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
		{
			return EnemyStates.FelguardChase;
		}

		public override IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity)
		{
			return EnemyStates.FelguardGiveUp;
		}

		public override IFSMState<BaseZombie> GetHitState(BaseZombie entity)
		{
			return EnemyStates.FelguardHit;
		}

		public override IFSMState<BaseZombie> GetDieState(BaseZombie entity)
		{
			return EnemyStates.FelguardDie;
		}

		public override IFSMState<BaseZombie> GetDigState(BaseZombie entity)
		{
			return GetGiveUpState(entity);
		}

		public override float GetDamageTypeMultiplier(DamageType damageType, bool headShot)
		{
			float num = 1f;
			if ((damageType & DamageType.PIERCING) != 0)
			{
				num *= 0.5f;
			}
			else if ((damageType & DamageType.SHOTGUN) != 0)
			{
				num *= 1.5f;
			}
			else if ((damageType & DamageType.BLADE) != 0)
			{
				num *= 0.75f;
			}
			if (headShot)
			{
				num *= 2f;
			}
			return num;
		}
	}
}
