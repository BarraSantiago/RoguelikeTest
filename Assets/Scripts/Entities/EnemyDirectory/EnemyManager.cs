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
        [SerializeField] public GameObject[] enemyPrefabs;

        [Header("Pooling Settings")]
        [SerializeField] public int initialPoolSize = 10;
        [SerializeField] public Transform enemyContainer;

        [Header("Spawn Settings")]
        [SerializeField] public float spawnRadius = 10f;
        [SerializeField] public int maxEnemies = 20;
        [SerializeField] private float eliteSpawnChance = 0.15f;
        [SerializeField] private float enemyScalingPerMinute = 0.2f;

        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private int enemiesPerFrame = 10;

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
        private float _gameStartTime;

        public static List<Enemy> Enemies => _activeEnemies;

        private void Awake()
        {
            InitializePools();
            Enemy.EnemyManager = this;
            Enemy.ExperiencePool = FindFirstObjectByType<ExperiencePool>();
            _gameStartTime = Time.time;
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

                EnemyPool pool = new EnemyPool();
                pool.Initialize(enemyPrefabs[i], initialPoolSize, enemyContainer);

                prefabEnemy.OnDeath += HandleEnemyDeath;

                _enemyPools[i] = pool;
                _enemyFactories[i] = new EnemyFactory(pool);
            }
        }

        private float GetEnemyScaling()
        {
            float minutesElapsed = (Time.time - _gameStartTime) / 60f;
            return 1f + (minutesElapsed * enemyScalingPerMinute);
        }

        public Enemy SpawnEnemy(int enemyType, Vector3 position)
        {
            if (_activeEnemies.Count >= maxEnemies) return null;

            IEnemy enemyInterface = _enemyFactories[enemyType].CreateEnemy(position);
            Enemy enemy = enemyInterface as Enemy;

            if (!enemy) return null;

            enemy.ResetEnemy(_enemyDataTemplates[enemyType]);

            float scaling = GetEnemyScaling();
            enemy.data.health *= scaling;
            enemy.data.attackDamage *= scaling;

            if (Random.value < eliteSpawnChance)
            {
                enemy.SetEnemyType(EnemyType.Elite);
                enemy.data.health *= 2f;
                enemy.data.attackDamage *= 1.5f;
            }

            _activeEnemies.Add(enemy);

            return enemy;
        }

        public void HandleEnemyDeath(Enemy enemy)
        {
            if (!_activeEnemies.Contains(enemy)) return;
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

                if (!enemy || !enemy.gameObject.activeInHierarchy)
                    continue;

                UpdateSingleEnemy(enemy, playerPosition);
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
            else
            {
                Vector2 direction = toPlayer / Mathf.Sqrt(sqrDistance);
                enemy.Move(direction);
            }
        }

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

        public void SpawnEnemiesAroundPlayer(int count)
        {
            if (!_playerTransform || !_player) return;

            float curseMultiplier = 1f + _player.PlayerStats.Curse + _player.statsMod.curseBonus;
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
                        _player.PlayerStats.Curse,
                        _player.statsMod.curseBonus
                    );
                }
            }
        }
    }
}