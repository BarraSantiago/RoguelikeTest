using PlayerDirectory;
using Utilities;

namespace ClassesDirectory.RangerClass
{
    public class Ranger : RangerBase
    {
        private const float SpreadAngle = 15f;

        public override void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod);
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerBaseStats, baseStatsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle));
        }

        public override void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod, 1.4f);
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerBaseStats, baseStatsMod, false);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle));
        }

        public override void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            PerformDodge(2f, playerBaseStats, baseStatsMod, vec);
        }
    }
}