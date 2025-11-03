using UnityEngine;
using Entities.EnemyDirectory;
using PlayerDirectory;

namespace Utilities
{
    public static class CombatUtilities
    {
        private static readonly Collider2D[] ColliderBuffer = new Collider2D[64];
        private static Enemy[] _enemyCache;
        private static float _lastCacheTime;
        private const float CacheLifetime = 0.1f;

        public static Enemy[] GetEnemiesInRadius(Vector2 position, float radius)
        {
            int count = Physics2D.OverlapCircleNonAlloc(position, radius, ColliderBuffer);
            int enemyCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (ColliderBuffer[i].CompareTag("Enemy"))
                {
                    enemyCount++;
                }
            }

            Enemy[] enemies = new Enemy[enemyCount];
            int index = 0;

            for (int i = 0; i < count && index < enemyCount; i++)
            {
                if (ColliderBuffer[i].CompareTag("Enemy"))
                {
                    enemies[index++] = ColliderBuffer[i].GetComponent<Enemy>();
                }
            }

            return enemies;
        }

        public static Enemy[] GetEnemiesInCone(Vector2 position, Vector2 direction, float radius, float angle)
        {
            Enemy[] inRadius = GetEnemiesInRadius(position, radius);
            float halfAngle = angle * 0.5f;
            int validCount = 0;

            for (int i = 0; i < inRadius.Length; i++)
            {
                Vector2 toTarget = ((Vector2)inRadius[i].transform.position - position).normalized;
                if (Vector2.Angle(direction, toTarget) <= halfAngle)
                {
                    inRadius[validCount++] = inRadius[i];
                }
            }

            if (validCount == inRadius.Length) return inRadius;

            Enemy[] result = new Enemy[validCount];
            System.Array.Copy(inRadius, result, validCount);
            return result;
        }

        public static float DamageEnemies(Enemy[] enemies, Stats playerStats, StatsModifier statsMod)
        {
            float totalDamage = 0f;
            float damage = CalculateDamage(playerStats, statsMod);

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i])
                {
                    enemies[i].TakeDamage(damage);
                    totalDamage += damage;
                }
            }
            return totalDamage;
        }

        public static void KnockbackEnemies(Enemy[] enemies, Vector2 origin, float force)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i]) continue;
                Vector2 knockbackDirection = ((Vector2)enemies[i].transform.position - origin).normalized;
                enemies[i].Knockback(knockbackDirection, force);
            }
        }

        public static Enemy GetClosestEnemy(Vector2 position, float maxRange)
        {
            Enemy[] enemies = GetEnemiesInRadius(position, maxRange);
            if (enemies.Length == 0) return null;

            Enemy closest = enemies[0];
            float closestDistSqr = Vector2.SqrMagnitude((Vector2)closest.transform.position - position);

            for (int i = 1; i < enemies.Length; i++)
            {
                float distSqr = Vector2.SqrMagnitude((Vector2)enemies[i].transform.position - position);
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = enemies[i];
                }
            }

            return closest;
        }

        public static float CalculateDamage(Stats playerStats, StatsModifier statsMod, float extraMod = 1.0f)
        {
            float baseDamage = playerStats.damage * statsMod.damageMod;

            if (IsCriticalHit(playerStats, statsMod))
            {
                baseDamage *= playerStats.criticalDamage * statsMod.criticalDamageMod;
            }

            return baseDamage * extraMod;
        }

        public static bool IsCriticalHit(Stats playerStats, StatsModifier statsMod)
        {
            return Random.value < playerStats.criticalChance * statsMod.criticalChanceMod;
        }

        public static Enemy GetNearestEnemy(Vector2 position, float maxRange)
        {
            if (Time.time - _lastCacheTime > CacheLifetime)
            {
                _enemyCache = Object.FindObjectsOfType<Enemy>();
                _lastCacheTime = Time.time;
            }

            Enemy nearestEnemy = null;
            float nearestDistSqr = maxRange * maxRange;

            for (int i = 0; i < _enemyCache.Length; i++)
            {
                if (!_enemyCache[i]) continue;

                float distSqr = Vector2.SqrMagnitude((Vector2)_enemyCache[i].transform.position - position);
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestEnemy = _enemyCache[i];
                }
            }

            return nearestEnemy;
        }
    }
}