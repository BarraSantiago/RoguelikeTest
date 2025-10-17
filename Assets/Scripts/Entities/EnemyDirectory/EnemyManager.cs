using System.Collections.Generic;
using Entities.XpDirectory;
using PlayerDirectory;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Prefabs")] 
        public GameObject[] enemyPrefabs;

        [Header("Pooling Settings")] 
        public int initialPoolSize = 10;
        public Transform enemyContainer;

        [Header("Spawn Settings")] 
        public float spawnRadius = 10f;
        public int maxEnemies = 20;

        private Player _player;
        private Transform _playerTransform;
        private Dictionary<int, Queue<GameObject>> _enemyPools = new();
        private Dictionary<int, EnemyData> _enemyDataTemplates = new();
        private List<Enemy> _activeEnemies = new();

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

        private void Update()
        {
            UpdateEnemyBehaviors();
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

        public void SpawnEnemiesAroundPlayer(int count)
        {
            if (!_playerTransform) return;

            for (int i = 0; i < count; i++)
            {
                int randomType = Random.Range(0, enemyPrefabs.Length);
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = _playerTransform.position +
                                   new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;

                SpawnEnemy(randomType, spawnPos);
            }
        }
    }
}