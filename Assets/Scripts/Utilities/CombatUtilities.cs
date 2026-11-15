using UnityEngine;
using Entities.EnemyDirectory;
using PlayerDirectory;

namespace Utilities
{
    public static class CombatUtilities
    {
        private static Player _cachedPlayer;

        private static Player CachedPlayer
        {
            get
            {
                if (!_cachedPlayer)
                {
                    _cachedPlayer = Object.FindFirstObjectByType<Player>();
                }

                return _cachedPlayer;
            }
        }

        public static Enemy[] GetEnemiesInRadius(Vector2 position, float radius)
        {
            if (EnemyManager.Enemies == null || EnemyManager.Enemies.Count == 0) return new Enemy[0];

            float radiusSqr = radius * radius;
            int validCount = 0;
            Enemy[] buffer = new Enemy[EnemyManager.Enemies.Count];

            for (int i = 0; i < EnemyManager.Enemies.Count; i++)
            {
                Enemy enemy = EnemyManager.Enemies[i];
                if (!enemy || !enemy.gameObject.activeInHierarchy) continue;

                float distSqr = Vector2.SqrMagnitude((Vector2)enemy.transform.position - position);
                if (distSqr <= radiusSqr)
                {
                    buffer[validCount++] = enemy;
                }
            }

            if (validCount == 0) return new Enemy[0];

            Enemy[] result = new Enemy[validCount];
            System.Array.Copy(buffer, result, validCount);
            return result;
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

        public static float DamageEnemies(Enemy[] enemies, BaseStats playerBaseStats, BaseStatsMod baseStatsMod)
        {
            float totalDamage = 0f;
        
            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i] || enemies[i].IsDead()) continue;

                // Roll the crit exactly once per hit (CalculateDamage would roll its own)
                float damage = playerBaseStats.Damage * baseStatsMod.damageMod;
                bool isCrit = IsCriticalHit(playerBaseStats, baseStatsMod);

                if (isCrit)
                {
                    damage *= playerBaseStats.CriticalDamage * baseStatsMod.criticalDamageMod;
                }

                enemies[i].TakeDamage(damage, isCrit);
                totalDamage += damage;

                Player player = CachedPlayer;
                if (player) player.OnDamageDealt(damage, isCrit);
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

        /// <summary>
        /// Non-critical damage. Crits are rolled once at hit time (DamageEnemy / DamageEnemies),
        /// so they must not be applied here as well.
        /// </summary>
        public static float CalculateDamage(BaseStats playerBaseStats, BaseStatsMod baseStatsMod, float extraMod = 1.0f)
        {
            return playerBaseStats.Damage * baseStatsMod.damageMod * extraMod;
        }

        public static bool IsCriticalHit(BaseStats playerBaseStats, BaseStatsMod baseStatsMod)
        {
            return Random.value < playerBaseStats.CriticalChance * baseStatsMod.criticalChanceMod;
        }

        public static Enemy GetNearestEnemy(Vector2 position, float maxRange)
        {
            if (EnemyManager.Enemies == null) return null;
            return EnemyManager.GetNearestEnemy(position, maxRange);
        }
        
        public static bool RollLuck(float baseLuck, float luckMod)
        {
            float totalLuck = baseLuck * luckMod;
            // Each point of luck = 1% chance, capped at 95%
            float luckChance = Mathf.Min(totalLuck * 0.01f, 0.95f);
            return Random.value < luckChance;
        }

        public static float ApplyGreed(float baseValue, float greed, float greedMod)
        {
            return baseValue * greed * greedMod;
        }

        public static float ApplyCurse(float baseValue, float curse, float curseMod)
        {
            return baseValue * (1f + (curse + curseMod));
        }
        
        // In DamageEnemy method:
        public static void DamageEnemy(Enemy enemy, float damage, bool isCrit, BaseStats playerBaseStats, BaseStatsMod baseStatsMod)
        {
            if (!enemy || enemy.IsDead()) return;
        
            float finalDamage = damage;
            
            if (isCrit)
            {
                finalDamage *= playerBaseStats.CriticalDamage * baseStatsMod.criticalDamageMod;
            }
            
            enemy.TakeDamage(finalDamage, isCrit);
            
            // Track damage
            Player player = CachedPlayer;
            if (player) player.OnDamageDealt(finalDamage, isCrit);
        }
    }
}