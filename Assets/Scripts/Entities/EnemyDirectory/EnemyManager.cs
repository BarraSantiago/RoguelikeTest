using System.Collections.Generic;
using Entities.XpDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Entities.EnemyDirectory
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Prefabs")] 
        [SerializeField]public GameObject[] enemyPrefabs;
        
        [Header("Pooling Settings")] 
        [SerializeField]public int initialPoolSize = 10;
        [SerializeField]public Transform enemyContainer;

        [Header("Spawn Settings")] 
        [SerializeField]public float spawnRadius = 10f;
        [SerializeField]public int maxEnemies = 20;
        
        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.1f; // Update enemies 10 times per second
        [SerializeField] private int enemiesPerFrame = 10; // Process enemies in batches
        
        public int ActiveEnemyCount => _activeEnemies.Count;
        public Transform PlayerTransform => _playerTransform;
        private float _lastUpdateTime;
        private int _currentUpdateIndex;
        private Player _player;
        private Transform _playerTransform;
        private Dictionary<int, Queue<GameObject>> _enemyPools = new();
        private Dictionary<int, EnemyData> _enemyDataTemplates = new();
        private static List<Enemy> _activeEnemies = new();
        
        public static List<Enemy> Enemies => _activeEnemies;
        
        private void Awake()
        {
            InitializePools();
            Enemy.EnemyManager = this;
            Enemy.ExperiencePool = FindFirstObjectByType<ExperiencePool>();
        }

        private void Start()
        {
            // Cache player reference
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                _player = playerObj.GetComponent<Player>();
                _playerTransform = playerObj.transform;
            }

            SpawnEnemiesAroundPlayer(10);
        }
        
        
        private void Update()
        {
            if (Time.time - _lastUpdateTime < updateInterval)
                return;
        
            _lastUpdateTime = Time.time;
            UpdateEnemyBehaviorsBatched();
        }

        private void InitializePools()
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                Enemy prefabEnemy = enemyPrefabs[i].GetComponent<Enemy>();
                _enemyDataTemplates[i] = prefabEnemy.data;
                prefabEnemy.OnDeath += HandleEnemyDeath;

                Queue<GameObject> pool = new Queue<GameObject>();
                for (int j = 0; j < initialPoolSize; j++)
                {
                    GameObject enemy = CreateNewEnemy(i);
                    enemy.SetActive(false);
                    pool.Enqueue(enemy);
                }

                _enemyPools[i] = pool;
            }
        }

        private GameObject CreateNewEnemy(int enemyType)
        {
            GameObject enemy = Instantiate(enemyPrefabs[enemyType], enemyContainer);
            enemy.GetComponent<Enemy>().data = _enemyDataTemplates[enemyType];
            return enemy;
        }

        public Enemy SpawnEnemy(int enemyType, Vector3 position)
        {
            if (_activeEnemies.Count >= maxEnemies)
                return null;

            GameObject enemyObj;
            Queue<GameObject> pool = _enemyPools[enemyType];

            if (pool.Count > 0)
            {
                enemyObj = pool.Dequeue();
                enemyObj.transform.position = position;

                Enemy enemy = enemyObj.GetComponent<Enemy>();
                enemy.ResetEnemy(_enemyDataTemplates[enemyType]);

                enemyObj.SetActive(true);
            }
            else
            {
                enemyObj = CreateNewEnemy(enemyType);
                enemyObj.transform.position = position;
            }

            Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
            _activeEnemies.Add(enemyComponent);

            return enemyComponent;
        }

        public void HandleEnemyDeath(Enemy enemy)
        {
            if (!_activeEnemies.Contains(enemy)) return;
            _activeEnemies.Remove(enemy);

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (!enemy.gameObject.name.Contains(enemyPrefabs[i].name)) continue;
                enemy.gameObject.SetActive(false);
                _enemyPools[i].Enqueue(enemy.gameObject);
                break;
            }
        }

        
        // Process enemies in batches to spread load
        private void UpdateEnemyBehaviorsBatched()
        {
            if (!_playerTransform || _activeEnemies.Count == 0) return;
        
            Vector2 playerPosition = _playerTransform.position;
            int enemiesThisFrame = Mathf.Min(enemiesPerFrame, _activeEnemies.Count);
            
            for (int i = 0; i < enemiesThisFrame; i++)
            {
                _currentUpdateIndex = (_currentUpdateIndex + 1) % _activeEnemies.Count;
                Enemy enemy = _activeEnemies[_currentUpdateIndex];
                
                if (!enemy || !enemy.gameObject.activeInHierarchy)
                    continue;
        
                UpdateSingleEnemy(enemy, playerPosition);
            }
        }
        
        // Optimize distance calculations with squared distance
        private void UpdateSingleEnemy(Enemy enemy, Vector2 playerPosition)
        {
            Vector2 enemyPosition = enemy.transform.position;
            Vector2 toPlayer = playerPosition - enemyPosition;
            float sqrDistance = toPlayer.sqrMagnitude;
            float attackRangeSqr = enemy.data.attackRange * enemy.data.attackRange;
        
            if (sqrDistance <= attackRangeSqr)
            {
                if (Time.time >= enemy.data.lastAttackTime + enemy.data.attackCooldown)
                {
                    _player.TakeDamage(enemy.data.attackDamage);
                    enemy.data.lastAttackTime = Time.time;
                }
            }
            else
            {
                // Use pre-calculated direction
                Vector2 direction = toPlayer / Mathf.Sqrt(sqrDistance);
                enemy.Move(direction);
            }
        }
        
        // Add spatial partitioning for better performance
        public static Enemy GetNearestEnemy(Vector2 position, float maxRange)
        {
            Enemy nearest = null;
            float minSqrDistance = maxRange * maxRange;
        
            foreach (Enemy enemy in _activeEnemies)
            {
                if (!enemy.gameObject.activeInHierarchy) continue;
                
                Vector2 offset = (Vector2)enemy.transform.position - position;
                float sqrDistance = offset.sqrMagnitude;
                
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = enemy;
                }
            }
        
            return nearest;
        }

        private void UpdateEnemyBehaviors()
        {
            if (!_playerTransform) return;

            foreach (Enemy enemy in _activeEnemies)
            {
                float distanceToPlayer = Vector2.Distance(enemy.transform.position, _playerTransform.position);

                if (distanceToPlayer <= enemy.data.attackRange)
                {
                    if (!(Time.time >= enemy.data.lastAttackTime + enemy.data.attackCooldown)) continue;
                    _player.TakeDamage(enemy.data.attackDamage);
                    enemy.data.lastAttackTime = Time.time;
                }
                else
                {
                    Vector2 direction = (_playerTransform.position - enemy.transform.position).normalized;
                    enemy.Move(direction);
                }
            }
        }

        // Add to EnemyManager.cs
        public void SpawnEnemiesAroundPlayer(int count)
        {
            if (!_playerTransform || !_player) return;
        
            // Apply curse to spawn count
            float curseMultiplier = 1f + _player.PlayerStats.Curse + _player.statsMod.curseBonus;
            int cursedCount = Mathf.RoundToInt(count * curseMultiplier);
        
            for (int i = 0; i < cursedCount; i++)
            {
                int randomType = Random.Range(0, enemyPrefabs.Length);
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = _playerTransform.position +
                                   new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;
        
                Enemy enemy = SpawnEnemy(randomType, spawnPos);
                
                // Apply curse to enemy health
                if (enemy)
                {
                    enemy.data.health = CombatUtilities.ApplyCurse(
                        enemy.data.health, 
                        _player.PlayerStats.Curse, 
                        _player.statsMod.curseBonus
                    );
                }
            }
        }
    }
}