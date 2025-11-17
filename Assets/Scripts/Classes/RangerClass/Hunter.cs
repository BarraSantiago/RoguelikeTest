using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Hunter : IClass
    {
        public ArrowPool ArrowPool;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            float range = playerStats.range * statsMod.rangeMod;
            Vector2 velocity = vec.Direction.normalized * (playerStats.projectileSpeed * statsMod.projectileSpeedMod);

            Arrow arrow = ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range, false);
            arrow.isHoming = true;
            arrow.slowChance = 0.3f;
            arrow.slowDuration = 2f;
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float spreadAngle = 30f;
            Vector2[] directions = new Vector2[3];

            directions[0] = vec.Direction;
            directions[1] = Quaternion.Euler(0, 0, spreadAngle) * vec.Direction;
            directions[2] = Quaternion.Euler(0, 0, -spreadAngle) * vec.Direction;

            float damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 0.8f);
            float range = playerStats.range * statsMod.rangeMod;
            float speed = playerStats.projectileSpeed * statsMod.projectileSpeedMod;

            foreach (Vector2 dir in directions)
            {
                Vector2 velocity = dir.normalized * speed;
                ArrowPool.GetArrowWithSetup(vec.Position, velocity, damage, range, false);
            }
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
                dodgeDirection = -vec.Direction;

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2.5f * statsMod.speedMod;
        }
    }
}