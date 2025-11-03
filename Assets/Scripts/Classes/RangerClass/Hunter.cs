using PlayerDirectory;
using UnityEngine;
using Utilities;
using Entities.EnemyDirectory;

namespace Classes.RangerClass
{
    public class Hunter : IClass
    {
        public ArrowPool ArrowPool;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject arrow = ArrowPool.GetArrow();
            Arrow arrowScript = arrow.GetComponent<Arrow>();

            arrowScript.damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            arrowScript.range = playerStats.range * statsMod.rangeMod;
            arrowScript.isHoming = true;
            arrowScript.slowChance = 0.3f;
            arrowScript.slowDuration = 2f;

            arrow.transform.position = vec.Position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float spreadAngle = 30f;
            Vector2[] directions = new Vector2[3];

            directions[0] = vec.Direction;
            directions[1] = Quaternion.Euler(0, 0, spreadAngle) * vec.Direction;
            directions[2] = Quaternion.Euler(0, 0, -spreadAngle) * vec.Direction;

            foreach (Vector2 dir in directions)
            {
                GameObject arrow = ArrowPool.GetArrow();
                Arrow arrowScript = arrow.GetComponent<Arrow>();

                arrowScript.damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 0.8f);
                arrowScript.range = playerStats.range * statsMod.rangeMod;

                arrow.transform.position = vec.Position;
                arrow.GetComponent<Rigidbody2D>().linearVelocity =
                    dir.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
            }
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

            if (dodgeDirection == Vector2.zero)
                dodgeDirection = -vec.Direction;

            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2.5f * statsMod.speedMod;
        }
    }
}