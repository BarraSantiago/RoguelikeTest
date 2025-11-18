using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Sniper : IClass
    {
        public ArrowPool ArrowPool;
        private const float LongRangeThreshold = 0.7f;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float maxRange = playerStats.range * statsMod.rangeMod;

            Enemy[] nearbyEnemies = CombatUtilities.GetEnemiesInRadius(vec.Position, 3f);
            float critMultiplier = nearbyEnemies.Length == 0 ? 2f : 1f;

            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod) * critMultiplier;
            Vector2 velocity = vec.Direction.normalized * (playerStats.projectileSpeed * statsMod.projectileSpeedMod);

            Arrow arrow = ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, maxRange);
            arrow.isSniper = true;
            arrow.sniperThreshold = maxRange * LongRangeThreshold;
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 2f);
            float range = playerStats.range * statsMod.rangeMod * 1.5f;
            Vector2 velocity = vec.Direction.normalized *
                               (playerStats.projectileSpeed * statsMod.projectileSpeedMod * 1.5f);

            Arrow arrow = ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range);
            arrow.maxPierceCount = 5;
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
                dodgeDirection = -vec.Direction;

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 3f * statsMod.speedMod;
        }
    }
}