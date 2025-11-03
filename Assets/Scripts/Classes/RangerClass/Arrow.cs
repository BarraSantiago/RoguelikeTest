using System.Collections;
using System.Collections.Generic;
using Classes.RangerClass;
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
        public int maxPierceCount = -1; // -1 = infinite
        private int _currentPierceCount;

        [Header("Sniper")]
        public bool isSniper;
        public float sniperThreshold;
        private Vector2 _startPosition;

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

        private HashSet<Enemy> _hitEnemies = new();
        private Rigidbody2D _rigidbody;
        private Transform _nearestEnemy;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _startPosition = transform.position;
            _currentPierceCount = 0;
            _hitEnemies.Clear();
            _nearestEnemy = null;
        }

        private void Update()
        {
            // Range check
            if (Vector2.Distance(_startPosition, transform.position) >= range)
            {
                OnArrowExpire();
                return;
            }

            // Homing logic for Hunter
            if (isHoming)
            {
                UpdateHoming();
            }
        }

        private void UpdateHoming()
        {
            if (!_nearestEnemy || !_nearestEnemy.gameObject.activeInHierarchy)
            {
                FindNearestEnemy();
            }

            if (_nearestEnemy)
            {
                Vector2 direction = ((Vector2)_nearestEnemy.position - (Vector2)transform.position).normalized;
                Vector2 currentVelocity = _rigidbody.linearVelocity;
                Vector2 newVelocity = Vector2.Lerp(currentVelocity, direction * currentVelocity.magnitude,
                    homingStrength * Time.deltaTime);

                _rigidbody.linearVelocity = newVelocity;

                // Rotate arrow to face direction
                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void FindNearestEnemy()
        {
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            float nearestDistance = float.MaxValue;
            Transform nearest = null;

            foreach (Enemy enemy in enemies)
            {
                if (!enemy.gameObject.activeInHierarchy || _hitEnemies.Contains(enemy)) continue;

                float distance = Vector2.Distance(transform.position, enemy.Position);
                if (distance < nearestDistance && distance < 10f) // 10 unit homing range
                {
                    nearestDistance = distance;
                    nearest = enemy.transform;
                }
            }

            _nearestEnemy = nearest;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null || _hitEnemies.Contains(enemy)) return;

            _hitEnemies.Add(enemy);

            // Calculate damage with Sniper bonus
            float finalDamage = damage;
            if (isSniper)
            {
                float distanceTraveled = Vector2.Distance(_startPosition, transform.position);
                if (distanceTraveled >= sniperThreshold)
                {
                    finalDamage *= 1.5f; // +50% damage at long range
                    ShowSniperEffect();
                }
            }

            enemy.TakeDamage(finalDamage);

            // Hunter slow effect
            if (slowChance > 0 && Random.value <= slowChance)
            {
                enemy.Slow(0.7f, slowDuration);
            }

            // Trapper leave trap
            if (leaveTrap)
            {
                CreateTrap(transform.position);
            }

            // Piercing logic
            if (isPiercing)
            {
                _currentPierceCount++;
                damage *= 0.9f; // 10% damage reduction per pierce

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
            trapZone.Initialize(trapRadius, trapDuration, false); // Slowing trap

            trapperReference?.RegisterTrap(trap);
        }

        private void ShowSniperEffect()
        {
            // Visual feedback for long-range hit
            GameObject effect = new GameObject("SniperHit");
            effect.transform.position = transform.position;

            SpriteRenderer sprite = effect.AddComponent<SpriteRenderer>();
            sprite.sprite = Resources.Load<Sprite>("Effects/SniperHit"); // Create this sprite
            sprite.color = new Color(1, 1, 0, 0.8f);

            Destroy(effect, 0.3f);
        }

        private void OnArrowExpire()
        {
            if (leaveTrap && trapperReference != null)
            {
                CreateTrap(transform.position);
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _hitEnemies.Clear();
            _nearestEnemy = null;
            gameObject.SetActive(false);
            FindAnyObjectByType<ArrowPool>()?.ReturnArrow(gameObject);
        }

        private void OnDisable()
        {
            // Reset all flags when returned to pool
            isPiercing = false;
            isSniper = false;
            isHoming = false;
            leaveTrap = false;
            maxPierceCount = -1;
            slowChance = 0;
            trapperReference = null;
        }
    }
}