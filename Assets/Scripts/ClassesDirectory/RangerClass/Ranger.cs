using PlayerDirectory;
using Utilities;

namespace ClassesDirectory.RangerClass
{
    public class Ranger : RangerBase
    {
        private const float SpreadAngle = 15f;

        public override void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            float range = playerStats.Range * statsMod.rangeMod;
            float speed = playerStats.ProjectileSpeed * statsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerStats, statsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle));
        }

        public override void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 1.4f);
            float range = playerStats.Range * statsMod.rangeMod;
            float speed = playerStats.ProjectileSpeed * statsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerStats, statsMod, false);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, range,
                projectileCount, SpreadAngle));
        }

        public override void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            PerformDodge(2f, playerStats, statsMod, vec);
        }
    }
}