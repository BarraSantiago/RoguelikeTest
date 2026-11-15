using System.Collections.Generic;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace WeaponsDirectory
{
    /// <summary>
    /// Per-run state of an owned weapon. Ticked by <see cref="WeaponInventory"/> from Player.Update,
    /// using scaled delta time, so weapons pause with the game and slow down with slow motion.
    ///
    /// The player's global stat upgrades apply to every weapon: damage, attack speed / cooldown
    /// reduction, area, projectile count, projectile speed, duration and knockback.
    /// </summary>
    public abstract class WeaponRuntime
    {
        public WeaponDefinition Definition { get; }
        public int Level { get; private set; } = 1;
        public bool CanLevelUp => Level < Definition.MaxLevel;

        protected readonly Player Player;
        protected readonly Transform PlayerTransform;
        protected readonly Transform Root;

        // Shared scratch list. Weapons tick one at a time on the main thread, so sharing is safe.
        private static readonly List<Enemy> EnemyBuffer = new();

        protected WeaponRuntime(WeaponDefinition definition, Player player, Transform root)
        {
            Definition = definition;
            Player = player;
            PlayerTransform = player.transform;
            Root = root;
        }

        public void LevelUp()
        {
            if (!CanLevelUp) return;
            Level++;
            OnLevelChanged();
        }

        public abstract void Tick(float deltaTime);

        protected virtual void OnLevelChanged() { }

        public virtual void Dispose()
        {
            if (Root) Object.Destroy(Root.gameObject);
        }

        // ---- Stats after the player's global modifiers ----

        protected WeaponLevelStats Stats => Definition.GetLevel(Level);
        protected PlayerBaseStatsMod Mods => Player.baseStatsMod;

        protected float Damage => Stats.damage * Mods.damageMod;
        protected float Area => Stats.area * Mods.areaMod;
        protected int Amount => Mathf.Max(1, Stats.amount + Mods.projectileCountBonus);
        protected float Speed => Stats.speed * Mods.projectileSpeedMod;
        protected float Duration => Stats.duration * Mods.durationMod;
        protected float Special => Stats.special;

        protected float Cooldown
        {
            get
            {
                float reduction = Mathf.Clamp(Player.PlayerBaseStats.CooldownReduction + Mods.cooldownReductionBonus,
                    0f, 0.8f);
                return Mathf.Max(0.05f, Stats.cooldown * Mods.attackSpeedMod * (1f - reduction));
            }
        }

        // ---- Combat helpers ----

        /// <summary>Damages an enemy with this weapon. Rolls the crit once and records session stats.</summary>
        protected void Hit(Enemy enemy, float damageMultiplier = 1f, float knockback = 0f, Vector2 knockbackFrom = default)
        {
            if (!enemy || enemy.IsDead() || !enemy.gameObject.activeInHierarchy) return;

            bool isCrit = CombatUtilities.IsCriticalHit(Player.PlayerBaseStats, Mods);
            CombatUtilities.DamageEnemy(enemy, Damage * damageMultiplier, isCrit, Player.PlayerBaseStats, Mods);

            if (knockback > 0f && !enemy.IsDead() && enemy.gameObject.activeInHierarchy)
            {
                Vector2 direction = enemy.Position - knockbackFrom;
                if (direction.sqrMagnitude < 0.0001f) direction = Random.insideUnitCircle;
                enemy.Knockback(direction.normalized, knockback * Mods.knockbackMod);
            }
        }

        /// <summary>
        /// Fills <paramref name="results"/> with live enemies whose body overlaps the circle.
        /// Always collect first and damage afterwards: a kill removes the enemy from
        /// EnemyManager.Enemies, which would break a loop over that list.
        /// </summary>
        protected static void CollectEnemies(Vector2 center, float radius, List<Enemy> results)
        {
            results.Clear();
            List<Enemy> enemies = EnemyManager.Enemies;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (!enemy || enemy.IsDead() || !enemy.gameObject.activeInHierarchy) continue;

                float reach = radius + enemy.HitRadius;
                if ((enemy.Position - center).sqrMagnitude <= reach * reach)
                {
                    results.Add(enemy);
                }
            }
        }

        /// <summary>Same as <see cref="CollectEnemies"/> but returns a shared buffer (valid until the next call).</summary>
        protected static List<Enemy> EnemiesInRadius(Vector2 center, float radius)
        {
            CollectEnemies(center, radius, EnemyBuffer);
            return EnemyBuffer;
        }

        protected static Enemy FindNearestEnemy(Vector2 from, float maxRange, HashSet<Enemy> exclude = null)
        {
            Enemy nearest = null;
            float bestSqr = maxRange * maxRange;
            List<Enemy> enemies = EnemyManager.Enemies;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (!enemy || enemy.IsDead() || !enemy.gameObject.activeInHierarchy) continue;
                if (exclude != null && exclude.Contains(enemy)) continue;

                float sqr = (enemy.Position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        protected Vector2 PlayerPosition => PlayerTransform.position;
    }
}
