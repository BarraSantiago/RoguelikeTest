using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Ranger : IClass
    {
        public ArrowPool ArrowPool;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            float range = playerStats.range * statsMod.rangeMod;
            Vector2 velocity = vec.Direction.normalized * (playerStats.projectileSpeed * statsMod.projectileSpeedMod);

            ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range, false);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 1.4f);
            float range = playerStats.range * statsMod.rangeMod;
            Vector2 velocity = vec.Direction.normalized * (playerStats.projectileSpeed * statsMod.projectileSpeedMod);

            ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range, true);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            Vector2 dodgeDirection = vec.Direction;
            if (dodgeDirection == Vector2.zero)
            {
                dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            }

            if (dodgeDirection != Vector2.zero)
            {
                player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2 * statsMod.speedMod;
            }
        }
    }
}