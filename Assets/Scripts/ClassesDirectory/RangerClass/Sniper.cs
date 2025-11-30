using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.RangerClass
{
    public class Sniper : RangerBase
    {
        private const float SpreadAngle = 10f;
        private const float LongRangeThreshold = 0.7f;

        public override void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float maxRange = playerStats.Range * statsMod.rangeMod;

            Enemy[] nearbyEnemies = CombatUtilities.GetEnemiesInRadius(vec.Position, 3f);
            float critMultiplier = nearbyEnemies.Length == 0 ? 2f : 1f;

            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod) * critMultiplier;
            float speed = playerStats.ProjectileSpeed * statsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerStats, statsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, maxRange,
                projectileCount, SpreadAngle, arrow =>
                {
                    arrow.isSniper = true;
                    arrow.sniperThreshold = maxRange * LongRangeThreshold;
                }));
        }

        public override void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 2f);
            float range = playerStats.Range * statsMod.rangeMod * 1.5f;
            float speed = playerStats.ProjectileSpeed * statsMod.projectileSpeedMod * 1.5f;
            Vector2 velocity = vec.Direction.normalized * speed;

            Arrow arrow = ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range);
            arrow.maxPierceCount = 5;
            arrow.isPiercing = true;
            arrow.isSniper = true;
            arrow.sniperThreshold = range * LongRangeThreshold;
        }

        public override void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            PerformDodge(3f, playerStats, statsMod, vec);
        }
    }
}