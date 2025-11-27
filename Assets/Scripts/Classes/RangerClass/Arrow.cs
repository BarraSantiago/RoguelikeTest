using System;
using System.Collections.Generic;
using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Classes.RangerClass
{
    public class Arrow : MonoBehaviour
    {
        [Header("Base Stats")] 
        public float damage;
        public float range;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private TrailRenderer trail;

        [Header("Piercing")] 
        public bool isPiercing;
        public int maxPierceCount = -1;
        private int _currentPierceCount;

        [Header("Sniper")] 
        public bool isSniper;
        public float sniperThreshold;
        [SerializeField] private float maxSniperMultiplier = 3f;
        [SerializeField] private float sniperExponent = 2f;

        [Header("Hunter")] 
        public bool isHoming;
        public float homingStrength = 2f;
        public float slowChance;
        public float slowDuration;

        [Header("Trapper")] 
        public bool leaveTrap;
        public float trapDuration;
        public float trapRadius;
        public Trapper trapperReference;

        // Public properties for ArrowPool to access
        public Vector2 StartPosition { get; private set; }
        public Vector2 Velocity { get; set; }
        public Transform NearestEnemy { get; private set; }

        private HashSet<Enemy> _hitEnemies = new();
        public static ArrowPool _pool;
        private static PlayerStats _playerStats;
        private static PlayerStatsModifier _statsMod;

        private void Awake()
        {
            _pool ??= ArrowPool.Instance;
            if (!trail) trail = GetComponent<TrailRenderer>();
        }

        public void Setup(Vector2 position, Vector2 velocity, float dmg, float range, bool piercing = false)
        {
            transform.position = position;
            StartPosition = position;
            Velocity = velocity;
            damage = dmg;
            this.range = range;
            isPiercing = piercing;
            _hitEnemies.Clear();
            NearestEnemy = null;


            if(_playerStats == null || _statsMod == null)
            {
                Player player = FindFirstObjectByType<Player>();
                if (player)
                {
                    _playerStats = player.playerStats;
                    _statsMod = player.statsMod;
                }
            }
            
            _currentPierceCount = _playerStats.pierceCount;
            lifetime = 5f * _playerStats.duration * _statsMod.durationMod;
            // Rotate to face direction
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void FindNearestEnemy()
        {
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            float nearestDistance = float.MaxValue;
            Transform nearest = null;

            foreach (Enemy enemy in enemies)
            {
                if (!enemy.gameObject.activeInHierarchy || _hitEnemies.Contains(enemy)) continue;

                float distance = Vector2.Distance(transform.position, enemy.Position);
                if (distance < nearestDistance && distance < 10f)
                {
                    nearestDistance = distance;
                    nearest = enemy.transform;
                }
            }

            NearestEnemy = nearest;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (!enemy || _hitEnemies.Contains(enemy)) return;

            _hitEnemies.Add(enemy);

            float finalDamage = damage;
            bool isCrit = CombatUtilities.IsCriticalHit(_playerStats, _statsMod);

            if (isSniper)
            {
                float sniperMod = CalculateSniperDamageModifier();
                finalDamage *= sniperMod;

                if (sniperMod > 1.5f)
                {
                    ShowSniperEffect();
                }
            }

            // Use CombatUtilities to damage enemy
            CombatUtilities.DamageEnemy(enemy, finalDamage, isCrit, _playerStats, _statsMod);

            // Apply slow effect
            if (slowChance > 0 && Random.value <= slowChance)
            {
                enemy.Slow(0.7f, slowDuration);
            }

            // Create trap on hit
            if (leaveTrap)
            {
                CreateTrap(transform.position);
            }

            // Handle piercing
            if (isPiercing)
            {
                _currentPierceCount++;
                damage *= 0.9f; // Reduce damage per pierce

                if (maxPierceCount > 0 && _currentPierceCount >= maxPierceCount)
                {
                    ReturnToPool();
                }
            }
            else
            {
                ReturnToPool();
            }
        }

        private float CalculateSniperDamageModifier()
        {
            float distanceTraveled = Vector2.Distance(StartPosition, transform.position);

            if (distanceTraveled < sniperThreshold)
            {
                return 1f; // No bonus below threshold
            }

            // Calculate normalized progress (0 to 1) from threshold to max range
            float effectiveDistance = distanceTraveled - sniperThreshold;
            float maxEffectiveDistance = range - sniperThreshold;
            float progress = Mathf.Clamp01(effectiveDistance / maxEffectiveDistance);

            // Exponential scaling: y = 1 + (maxMultiplier - 1) * progress^exponent
            float multiplier = 1f + (maxSniperMultiplier - 1f) * Mathf.Pow(progress, sniperExponent);

            return multiplier;
        }

        private void ShowSniperEffect()
        {
            // Visual feedback for high damage sniper shot
            if (trail)
            {
                trail.startColor = new Color(1f, 0.5f, 0f); // Orange trail for bonus damage
            }
        }

        public void OnArrowExpire()
        {
            if (leaveTrap && trapperReference != null)
            {
                CreateTrap(transform.position);
            }

            ReturnToPool();
        }

        private void CreateTrap(Vector2 position)
        {
            GameObject trap = new GameObject("ArrowTrap");
            trap.transform.position = position;

            TrapZone trapZone = trap.AddComponent<TrapZone>();
            trapZone.Initialize(trapRadius, trapDuration, false);

            trapperReference?.RegisterTrap(trap);
        }

        private void ReturnToPool()
        {
            Velocity = Vector2.zero;
            _pool?.Return(this);
        }

        private void OnDisable()
        {
            isPiercing = false;
            isSniper = false;
            isHoming = false;
            leaveTrap = false;
            maxPierceCount = -1;
            slowChance = 0;
            trapperReference = null;
            _hitEnemies.Clear();
            NearestEnemy = null;
            Velocity = Vector2.zero;
        }

        private void OnDestroy()
        {
            _pool = null;
            _playerStats = null;
            _statsMod = null;
        }
    }
}