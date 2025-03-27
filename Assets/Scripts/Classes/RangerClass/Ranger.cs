using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Ranger : IClass

    {
        public ArrowPool arrowPool;

        // Normal arrow attack
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            GameObject arrow = arrowPool.GetArrow();
            arrow.GetComponent<Arrow>().damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            arrow.GetComponent<Arrow>().range = playerStats.range * statsMod.rangeMod;

            arrow.transform.position = direction.position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity =
                direction.direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
        }

        // Piercing arrow attack
        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            GameObject piercingArrow = arrowPool.GetArrow();
            piercingArrow.GetComponent<Arrow>().damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 1.4f);
            piercingArrow.GetComponent<Arrow>().isPiercing = true;
            piercingArrow.GetComponent<Arrow>().range = playerStats.range * statsMod.rangeMod;

            piercingArrow.transform.position = direction.position;
            piercingArrow.GetComponent<Rigidbody2D>().linearVelocity =
                direction.direction.normalized * playerStats.projectileSpeed;
        }

        // Dodge roll
        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2 * statsMod.speedMod;
            // Add invincibility frames or other defensive mechanics
        }
    }
}