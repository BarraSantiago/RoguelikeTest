using System.Collections.Generic;
using Entities;
using PlayerDirectory;
using UnityEngine;

namespace Classes.WarriorClass
{
    public class Warrior : IClass
    {
        public void BasicAttack(PlayerStats playerStats, Direction direction)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(direction.position, playerStats.range);
            foreach (var hitCollider in hitColliders)
            {
                Vector2 toTarget = ((Vector2)hitCollider.transform.position - direction.position).normalized;
                if (!(Vector2.Angle(direction.direction, toTarget) <= playerStats.area / 2)) continue;

                if (hitCollider.CompareTag("Enemy"))
                {
                    hitCollider.GetComponent<Enemy>().TakeDamage(playerStats.damage);
                }
            }
        }

        public void SpecialAttack(PlayerStats playerStats, Direction direction)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(direction.position, playerStats.range);
            foreach (var hitCollider in hitColliders)
            {
                if (!hitCollider.CompareTag("Enemy")) continue;
                hitCollider.GetComponent<Enemy>().TakeDamage(playerStats.damage * 1.5f); // Increased damage
            }
        }

        public void Defense(PlayerStats playerStats, Direction direction)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(direction.position, playerStats.range);

            foreach (var hitCollider in hitColliders)
            {
                if (!hitCollider.CompareTag("Enemy")) continue;
                
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                
                if (!enemy) continue;
                Vector2 knockbackDirection = ((Vector2)hitCollider.transform.position - direction.position).normalized;

                enemy.Knockback(knockbackDirection, playerStats.knockback);
            }
        }
    }
}