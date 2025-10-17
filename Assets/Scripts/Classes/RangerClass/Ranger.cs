using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Classes.RangerClass
{
    public class Ranger : IClass

    {
        public ArrowPool ArrowPool;

        // Normal arrow attack
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject arrow = ArrowPool.GetArrow();
            arrow.GetComponent<Arrow>().damage = CombatUtilities.CalculateDamage(playerStats, statsMod);
            arrow.GetComponent<Arrow>().range = playerStats.range * statsMod.rangeMod;

            arrow.transform.position = vec.Position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed * statsMod.projectileSpeedMod;
        }

        // Piercing arrow attack
        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject piercingArrow = ArrowPool.GetArrow();
            piercingArrow.GetComponent<Arrow>().damage = CombatUtilities.CalculateDamage(playerStats, statsMod, 1.4f);
            piercingArrow.GetComponent<Arrow>().isPiercing = true;
            piercingArrow.GetComponent<Arrow>().range = playerStats.range * statsMod.rangeMod;

            piercingArrow.transform.position = vec.Position;
            piercingArrow.GetComponent<Rigidbody2D>().linearVelocity =
                vec.Direction.normalized * playerStats.projectileSpeed;
        }

        // Dodge roll
        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2 * statsMod.speedMod;
            // Add invincibility frames or other defensive mechanics
        }
    }
}