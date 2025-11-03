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
            GameObject arrow = ArrowPool.GetArrow();
            Arrow arrowScript = arrow.GetComponent<Arrow>();

            float maxRange = playerStats.range * statsMod.rangeMod;
            arrowScript.range = maxRange;
            arrowScript.isSniper = true;
            arrowScript.sniperThreshold = maxRange * LongRangeThreshold;

            // Check if no enemies nearby for crit bonus
            Enemy[] nearbyEnemies = CombatUtilities.GetEnemiesInRadius(vec.Position, 3f);
            float critMultiplier = nearbyEnemies.Length == 0 ? 2f : 1f;

            arrowScript.damage = CombatUtilities.CalculateDamage(playerStats, statsMod) * critMultiplier;

            arrow.transform.position = vec.Position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject piercingArrow = ArrowPool.GetArrow();
            Arrow arrowScript = piercingArrow.GetComponent<Arrow>();

            arrowScript.damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 2f);
            arrowScript.isPiercing = true;
            arrowScript.maxPierceCount = 5;
            arrowScript.range = playerStats.range * statsMod.rangeMod * 1.5f;

            piercingArrow.transform.position = vec.Position;
            piercingArrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod * 1.5f;
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
                dodgeDirection = -vec.Direction;

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 3f * statsMod.speedMod;
        }
    }
}