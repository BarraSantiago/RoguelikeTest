using Classes.RangerClass;
using PlayerDirectory;
using UnityEngine;

namespace Classes
{
    public class Ranger : IClass
    {
        public ArrowPool arrowPool;

        // Normal arrow attack
        public void BasicAttack(PlayerStats playerStats, Direction direction)
        {
            GameObject arrow = arrowPool.GetArrow();
            arrow.GetComponent<Arrow>().damage = playerStats.damage;
            arrow.GetComponent<Arrow>().range = playerStats.range;
            
            arrow.transform.position = direction.position;
            arrow.GetComponent<Rigidbody2D>().linearVelocity = direction.direction.normalized * playerStats.projectileSpeed;
        }

        // Piercing arrow attack
        public void SpecialAttack(PlayerStats playerStats, Direction direction)
        {
            GameObject piercingArrow = arrowPool.GetArrow();
            piercingArrow.GetComponent<Arrow>().damage = playerStats.damage * 1.5f; // Increased damage
            piercingArrow.GetComponent<Arrow>().isPiercing = true;
            piercingArrow.GetComponent<Arrow>().range = playerStats.range;

            piercingArrow.transform.position = direction.position;
            piercingArrow.GetComponent<Rigidbody2D>().linearVelocity = direction.direction.normalized * playerStats.projectileSpeed;
        }

        // Dodge roll
        public void Defense(PlayerStats playerStats, Direction direction)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector2 dodgeDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            player.transform.position += (Vector3)dodgeDirection * playerStats.speed * 2; // Increased speed for dodge
            // Add invincibility frames or other defensive mechanics
        }
    }
}