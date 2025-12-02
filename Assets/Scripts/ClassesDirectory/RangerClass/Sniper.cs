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

        public override void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float maxRange = playerBaseStats.Range * baseStatsMod.rangeMod;

            Enemy[] nearbyEnemies = CombatUtilities.GetEnemiesInRadius(vec.Position, 3f);
            float critMultiplier = nearbyEnemies.Length == 0 ? 2f : 1f;

            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod) * critMultiplier;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod;
            int projectileCount = GetProjectileCount(playerBaseStats, baseStatsMod);

            ArrowPool.StartCoroutine(FireArrowsSequential(vec.Position, vec.Direction, speed, damage, maxRange,
                projectileCount, SpreadAngle, arrow =>
                {
                    arrow.isSniper = true;
                    arrow.sniperThreshold = maxRange * LongRangeThreshold;
                }));
        }

        public override void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerBaseStats, baseStatsMod, 2f);
            float range = playerBaseStats.Range * baseStatsMod.rangeMod * 1.5f;
            float speed = playerBaseStats.ProjectileSpeed * baseStatsMod.projectileSpeedMod * 1.5f;
            Vector2 velocity = vec.Direction.normalized * speed;

            Arrow arrow = ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range);
            arrow.maxPierceCount = 5;
            arrow.isPiercing = true;
            arrow.isSniper = true;
            arrow.sniperThreshold = range * LongRangeThreshold;
        }

        public override void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            PerformDodge(3f, playerBaseStats, baseStatsMod, vec);
        }
    }
}