using System.Collections.Generic;
using Entities.Pickables;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Entities.EnemyDirectory
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] public GameObject[] enemyPrefabs;

        [Header("Pooling Settings")]
        [SerializeField] public int initialPoolSize = 10;
        [SerializeField] public Transform enemyContainer;

        [Header("Spawn Settings")]
        [SerializeField] public float spawnRadius = 10f;
        [SerializeField] public int maxEnemies = 20;

        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private int enemiesPerFrame = 10;

        [Header("Coin Drops")]
        [SerializeField] private float coinDropChance = 0.02f;
        [SerializeField] private float coinBaseValue = 1f;
        
        private CoinPool _coinPool;

        
        public int ActiveEnemyCount => _activeEnemies.Count;
        public Transform PlayerTransform => _playerTransform;

        private float _lastUpdateTime;
        private int _currentUpdateIndex;
        private Player _player;
        private Transform _playerTransform;
        private Dictionary<int, EnemyPool> _enemyPools = new();
        private Dictionary<int, EnemyFactory> _enemyFactories = new();
        private Dictionary<int, EnemyData> _enemyDataTemplates = new();
        private static List<Enemy> _activeEnemies = new();

        public static List<Enemy> Enemies => _activeEnemies;

        private void Awake()
        {
            InitializePools();
            Enemy.EnemyManager = this;
            Enemy.ExperiencePool = FindFirstObjectByType<ExperiencePool>();
            _coinPool = FindFirstObjectByType<CoinPool>();
        }

        private void Start()
        {
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
            float interval = PerformanceManager.Instance ? 
                PerformanceManager.Instance.GetUpdateInterval() : updateInterval;
                
            if (Time.time - _lastUpdateTime < interval)
                return;
        
            _lastUpdateTime = Time.time;
            UpdateEnemyBehaviorsBatched();
        }
        
        private void FixedUpdate()
        {
            if (!_playerTransform || _activeEnemies.Count == 0) return;
        
            Vector2 playerPosition = _playerTransform.position;
        
            foreach (Enemy enemy in _activeEnemies)
            {
                if (!enemy || !enemy.gameObject || !enemy.gameObject.activeInHierarchy)
                    continue;
        
                UpdateSingleEnemyMovement(enemy, playerPosition);
            }
        }
        
        

        private void InitializePools()
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                Enemy prefabEnemy = enemyPrefabs[i].GetComponent<Enemy>();
                _enemyDataTemplates[i] = prefabEnemy.data;

                EnemyPool pool = new EnemyPool();
                pool.Initialize(enemyPrefabs[i], initialPoolSize, enemyContainer);

                prefabEnemy.OnDeath += HandleEnemyDeath;

                _enemyPools[i] = pool;
                _enemyFactories[i] = new EnemyFactory(pool);
            }
        }

        public Enemy SpawnEnemy(int enemyType, Vector3 position)
        {
            if (_activeEnemies.Count >= maxEnemies) return null;

            IEnemy enemyInterface = _enemyFactories[enemyType].CreateEnemy(position);
            Enemy enemy = enemyInterface as Enemy;

            if (!enemy) return null;

            // Scaling, elite and boss rolls are applied by WaveSystem.ApplyScaling only
            enemy.ResetEnemy(_enemyDataTemplates[enemyType]);

            _activeEnemies.Add(enemy);

            return enemy;
        }

        public void HandleEnemyDeath(Enemy enemy)
        {
            if (!_activeEnemies.Contains(enemy)) return;
            
            HandleCoinDrop(enemy);
            
            _activeEnemies.Remove(enemy);
        
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (!enemy.gameObject.name.Contains(enemyPrefabs[i].name)) continue;
                _enemyPools[i].Return(enemy);
                break;
            }
        }

        private void UpdateEnemyBehaviorsBatched()
        {
            if (!_playerTransform || _activeEnemies.Count == 0) return;
        
            Vector2 playerPosition = _playerTransform.position;
            int enemiesThisFrame = Mathf.Min(enemiesPerFrame, _activeEnemies.Count);
        
            for (int i = 0; i < enemiesThisFrame; i++)
            {
                _currentUpdateIndex = (_currentUpdateIndex + 1) % _activeEnemies.Count;
                Enemy enemy = _activeEnemies[_currentUpdateIndex];
        
                if (!enemy || !enemy.gameObject || !enemy.gameObject.activeInHierarchy)
                    continue;
        
                UpdateSingleEnemy(enemy, playerPosition);
            }
        }

        private void UpdateSingleEnemyMovement(Enemy enemy, Vector2 playerPosition)
        {
            Vector2 enemyPosition = enemy.transform.position;
            Vector2 toPlayer = playerPosition - enemyPosition;
            float sqrDistance = toPlayer.sqrMagnitude;
            float attackRangeSqr = enemy.data.attackRange * enemy.data.attackRange;
        
            if (sqrDistance > attackRangeSqr)
            {
                // Normalize the direction properly
                Vector2 direction = toPlayer.normalized;
                enemy.Move(direction);
            }
            else
            {
                enemy.Move(Vector2.zero);
            }
        }
        
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
        }

        public static Enemy GetNearestEnemy(Vector2 position, float maxRange)
        {
            Enemy nearest = null;
            float minSqrDistance = maxRange * maxRange;
        
            // Clean up null references while iterating
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (!_activeEnemies[i] || !_activeEnemies[i].gameObject)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }
        
                Enemy enemy = _activeEnemies[i];
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

        public void SpawnEnemiesAroundPlayer(int count)
        {
            if (!_playerTransform || !_player) return;

            float curseMultiplier = 1f + _player.PlayerBaseStats.Curse + _player.baseStatsMod.curseBonus;
            int cursedCount = Mathf.RoundToInt(count * curseMultiplier);

            for (int i = 0; i < cursedCount; i++)
            {
                int randomType = Random.Range(0, enemyPrefabs.Length);
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = _playerTransform.position +
                                   new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;

                Enemy enemy = SpawnEnemy(randomType, spawnPos);

                if (enemy)
                {
                    enemy.data.health = CombatUtilities.ApplyCurse(
                        enemy.data.health,
                        _player.PlayerBaseStats.Curse,
                        _player.baseStatsMod.curseBonus
                    );
                }
            }
        }
        
        private void HandleCoinDrop(Enemy enemy)
        {
            if (!_player || !_coinPool) return;
        
            float totalLuck = _player.PlayerBaseStats.Luck * _player.baseStatsMod.luckMod;
            float totalGreed = _player.PlayerBaseStats.Greed * _player.baseStatsMod.greedMod;
        
            float finalDropChance = coinDropChance * totalLuck;
        
            if (Random.value < finalDropChance)
            {
                float coinValue = coinBaseValue * totalGreed;
                _coinPool.SpawnCoinDrop(enemy.transform.position, coinValue);
            }
        }
        
        private void OnDestroy()
        {
            _activeEnemies.Clear();
        }
    }
}