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

        private Player player;
        private Transform playerTransform;
        private Dictionary<int, Queue<GameObject>> enemyPools = new Dictionary<int, Queue<GameObject>>();
        private Dictionary<int, EnemyData> enemyDataTemplates = new Dictionary<int, EnemyData>();
        private List<Enemy> activeEnemies = new List<Enemy>();

        private void Awake()
        {
            InitializePools();
            Enemy.enemyManager = this;
            Enemy.experiencePool = FindFirstObjectByType<ExperiencePool>();
        }

        private void Start()
        {
            // Cache player reference
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                player = playerObj.GetComponent<Player>();
                playerTransform = playerObj.transform;
            }

            SpawnEnemiesAroundPlayer(10);
        }

        private void InitializePools()
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                Enemy prefabEnemy = enemyPrefabs[i].GetComponent<Enemy>();
                enemyDataTemplates[i] = prefabEnemy.data;
                prefabEnemy.OnDeath += HandleEnemyDeath;

                Queue<GameObject> pool = new Queue<GameObject>();
                for (int j = 0; j < initialPoolSize; j++)
                {
                    GameObject enemy = CreateNewEnemy(i);
                    enemy.SetActive(false);
                    pool.Enqueue(enemy);
                }

                enemyPools[i] = pool;
            }
        }

        private GameObject CreateNewEnemy(int enemyType)
        {
            GameObject enemy = Instantiate(enemyPrefabs[enemyType], enemyContainer);
            enemy.GetComponent<Enemy>().data = enemyDataTemplates[enemyType];
            return enemy;
        }

        public Enemy SpawnEnemy(int enemyType, Vector3 position)
        {
            if (activeEnemies.Count >= maxEnemies)
                return null;

            GameObject enemyObj;
            Queue<GameObject> pool = enemyPools[enemyType];

            if (pool.Count > 0)
            {
                enemyObj = pool.Dequeue();
                enemyObj.transform.position = position;

                Enemy enemy = enemyObj.GetComponent<Enemy>();
                enemy.ResetEnemy(enemyDataTemplates[enemyType]);

                enemyObj.SetActive(true);
            }
            else
            {
                enemyObj = CreateNewEnemy(enemyType);
                enemyObj.transform.position = position;
            }

            Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
            activeEnemies.Add(enemyComponent);

            return enemyComponent;
        }

        public void HandleEnemyDeath(Enemy enemy)
        {
            if (!activeEnemies.Contains(enemy)) return;
            activeEnemies.Remove(enemy);

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (!enemy.gameObject.name.Contains(enemyPrefabs[i].name)) continue;
                enemy.gameObject.SetActive(false);
                enemyPools[i].Enqueue(enemy.gameObject);
                break;
            }
        }

        private void Update()
        {
            UpdateEnemyBehaviors();
        }

        private void UpdateEnemyBehaviors()
        {
            if (!playerTransform) return;

            foreach (Enemy enemy in activeEnemies)
            {
                float distanceToPlayer = Vector2.Distance(enemy.transform.position, playerTransform.position);

                if (distanceToPlayer <= enemy.data.attackRange)
                {
                    if (!(Time.time >= enemy.data.lastAttackTime + enemy.data.attackCooldown)) continue;
                    player.TakeDamage(enemy.data.attackDamage);
                    enemy.data.lastAttackTime = Time.time;
                }
                else
                {
                    Vector2 direction = (playerTransform.position - enemy.transform.position).normalized;
                    enemy.Move(direction);
                }
            }
        }

        public void SpawnEnemiesAroundPlayer(int count)
        {
            if (!playerTransform) return;

            for (int i = 0; i < count; i++)
            {
                int randomType = Random.Range(0, enemyPrefabs.Length);
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = playerTransform.position +
                                   new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;

                SpawnEnemy(randomType, spawnPos);
            }
        }
    }
}