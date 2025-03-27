using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;

namespace Classes
{
    public interface IClass
    {
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);
        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);
        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);
    }

    public abstract class PlayerClass : IClass
    {
        public abstract void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);

        public abstract void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);

        public abstract void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction);

        public float CalculateDamage(PlayerStats playerStats, PlayerStatsModifier statsMod, float extraMod = 1.0f)
        {
            return playerStats.damage * statsMod.damageMod * (IsCriticalHit(playerStats, statsMod)
                ? playerStats.criticalDamage * statsMod.criticalDamageMod : 1) * extraMod;
        }

        public bool IsCriticalHit(PlayerStats playerStats, PlayerStatsModifier statsMod)
        {
            return Random.value < playerStats.criticalChance * statsMod.criticalChanceMod;
        }

        public Enemy[] GetEnemiesInRadius(Vector2 position, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
            colliders = System.Array.FindAll(colliders, collider => collider.CompareTag("Enemy"));
            Enemy[] enemies = new Enemy[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                enemies[i] = colliders[i].GetComponent<Enemy>();
            }
            return enemies;
        }

        public Enemy[] GetEnemiesInCone(Vector2 position, Vector2 direction, float radius, float angle)
        {
            Enemy[] inRadius = GetEnemiesInRadius(position, radius);
            
            return System.Array.FindAll(inRadius, collider =>
            {
                Vector2 toTarget = ((Vector2)collider.transform.position - position).normalized;
                return Vector2.Angle(direction, toTarget) <= angle / 2;
            });
        }

        public void DamageEnemies(Enemy[] enemies, PlayerStats playerStats, PlayerStatsModifier statsMod)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy)
                {
                    enemy.TakeDamage(CalculateDamage(playerStats, statsMod));
                }
            }
        }

        public void KnockbackEnemies(Enemy[] enemies, Vector2 origin, float force)
        {
            foreach (Enemy enemy in enemies)
            {
                if (!enemy) continue;
                Vector2 knockbackDirection = ((Vector2)enemy.transform.position - origin).normalized;
                enemy.Knockback(knockbackDirection, force);
            }
        }

        public Enemy GetClosestEnemy(Vector2 position, float maxRange)
        {
            Enemy[] enemies = GetEnemiesInRadius(position, maxRange);

            Enemy closest = null;
            float closestDist = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                float dist = Vector2.Distance(position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy.GetComponent<Enemy>();
                }
            }

            return closest;
        }
    }
}