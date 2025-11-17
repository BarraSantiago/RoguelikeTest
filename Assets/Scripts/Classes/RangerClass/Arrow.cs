using System.Collections.Generic;
using Entities.EnemyDirectory;
using UnityEngine;

namespace Classes.RangerClass
{
    public class Arrow : MonoBehaviour
    {
        [Header("Base Stats")] 
        public float damage;
        public float range;

        [Header("Piercing")] 
        public bool isPiercing;
        public int maxPierceCount = -1;
        private int _currentPierceCount;

        [Header("Sniper")] 
        public bool isSniper;
        public float sniperThreshold;

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

        private void Awake()
        {
            _pool ??= GetComponentInParent<ArrowPool>();
        }

        public void Setup(Vector2 position, Vector2 velocity, float dmg, float rng, bool piercing = false)
        {
            transform.position = position;
            StartPosition = position;
            Velocity = velocity;
            damage = dmg;
            range = rng;
            isPiercing = piercing;
            _currentPierceCount = 0;
            _hitEnemies.Clear();
            NearestEnemy = null;

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
            if (isSniper)
            {
                float distanceTraveled = Vector2.Distance(StartPosition, transform.position);
                if (distanceTraveled >= sniperThreshold)
                {
                    finalDamage *= 1.5f;
                    ShowSniperEffect();
                }
            }

            enemy.TakeDamage(finalDamage);

            if (slowChance > 0 && Random.value <= slowChance)
            {
                enemy.Slow(0.7f, slowDuration);
            }

            if (leaveTrap)
            {
                CreateTrap(transform.position);
            }

            if (isPiercing)
            {
                _currentPierceCount++;
                damage *= 0.9f;

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

        private void CreateTrap(Vector2 position)
        {
            GameObject trap = new GameObject("ArrowTrap");
            trap.transform.position = position;

            TrapZone trapZone = trap.AddComponent<TrapZone>();
            trapZone.Initialize(trapRadius, trapDuration, false);

            trapperReference?.RegisterTrap(trap);
        }

        private void ShowSniperEffect()
        {
            // Visual feedback placeholder
        }

        public void OnArrowExpire()
        {
            if (leaveTrap && trapperReference != null)
            {
                CreateTrap(transform.position);
            }

            ReturnToPool();
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
    }
}