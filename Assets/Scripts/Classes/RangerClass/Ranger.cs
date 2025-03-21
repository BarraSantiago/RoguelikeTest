using PlayerDirectory;
using UnityEngine;

namespace Classes
{
    public class Ranger : IClass
    {
        public ArrowPool arrowPool;

        // Normal arrow attack
        public void BasicAttack(PlayerStats playerStats)
        {
            GameObject arrow = arrowPool.GetArrow();
            arrow.GetComponent<Arrow>().damage = playerStats.damage;
            // Set arrow's position and direction
        }

        // Piercing arrow attack
        public void SpecialAttack(PlayerStats playerStats)
        {
            GameObject piercingArrow = arrowPool.GetArrow();
            piercingArrow.GetComponent<Arrow>().damage = playerStats.damage * 1.5f; // Increased damage
            piercingArrow.GetComponent<Arrow>().isPiercing = true;
            // Set arrow's position and direction
        }

        // Dodge roll
        public void Defense(PlayerStats playerStats)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2; // Increased speed for dodge
            // Add invincibility frames or other defensive mechanics
        }
    }
}