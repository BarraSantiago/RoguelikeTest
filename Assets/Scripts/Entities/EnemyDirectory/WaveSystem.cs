using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    [Serializable]
    public class WaveConfig
    {
        public float startTime;
        public float endTime;
        public int baseSpawnCount;
        public float spawnInterval;
        [Range(0, 1)] public float eliteChance;
        [Range(0, 1)] public float bossChance;
        public EnemyScaling scaling;
    }

    [Serializable]
    public class EnemyScaling
    {
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
    }

    public class WaveSystem : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [SerializeField] private List<WaveConfig> waves = new();
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private int maxSimultaneousEnemies = 100;

        [Header("Scaling Settings")]
        [SerializeField] private float healthScalingPerMinute = 0.25f;
        [SerializeField] private float damageScalingPerMinute = 0.2f;
        [SerializeField] private float speedScalingPerMinute = 0.1f;

        [Header("Mobile Optimization")]
        [SerializeField] private int maxSpawnsPerFrame = 8;

        private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();

        private struct SpawnRequest
        {
            public Vector3 Position;
            public EnemyScaling Scaling;
            public EnemyType Type;
            public int PrefabIndex;
        }

        private EnemyManager _enemyManager;
        private float _gameStartTime;
        private float _nextSpawnTime;
        private int _currentWaveIndex;
        private WaveConfig _currentWave;

        public float GameTime => Time.time - _gameStartTime;
        public int CurrentWaveNumber => _currentWaveIndex + 1;

        private void Awake()
        {
            _enemyManager = GetComponent<EnemyManager>();
            InitializeDefaultWaves();
        }

        private void Start()
        {
            _gameStartTime = Time.time;
            StartWave(0);
        }

        private void InitializeDefaultWaves()
        {
            if (waves.Count > 0) return;

            waves = new List<WaveConfig>
            {
                // Wave 1: 0-1 minute - Tutorial Wave
                new WaveConfig
                {
                    startTime = 0f,
                    endTime = 60f,
                    baseSpawnCount = 8,
                    spawnInterval = 1.8f,
                    eliteChance = 0.05f,
                    bossChance = 0f,
                    scaling = new EnemyScaling { healthMultiplier = 1f, damageMultiplier = 1f, speedMultiplier = 1f }
                },
                // Wave 2: 1-2 minutes - Early Game
                new WaveConfig
                {
                    startTime = 60f,
                    endTime = 120f,
                    baseSpawnCount = 15,
                    spawnInterval = 1.5f,
                    eliteChance = 0.15f,
                    bossChance = 0f,
                    scaling = new EnemyScaling { healthMultiplier = 1.5f, damageMultiplier = 1.4f, speedMultiplier = 1.1f }
                },
                // Wave 3: 2-3 minutes - Ramp Up
                new WaveConfig
                {
                    startTime = 120f,
                    endTime = 180f,
                    baseSpawnCount = 22,
                    spawnInterval = 1.2f,
                    eliteChance = 0.25f,
                    bossChance = 0.05f,
                    scaling = new EnemyScaling { healthMultiplier = 2f, damageMultiplier = 1.8f, speedMultiplier = 1.2f }
                },
                // Wave 4: 3-5 minutes - Mid Game
                new WaveConfig
                {
                    startTime = 180f,
                    endTime = 300f,
                    baseSpawnCount = 30,
                    spawnInterval = 1f,
                    eliteChance = 0.35f,
                    bossChance = 0.08f,
                    scaling = new EnemyScaling { healthMultiplier = 3f, damageMultiplier = 2.3f, speedMultiplier = 1.3f }
                },
                // Wave 5: 5-8 minutes - Late Game
                new WaveConfig
                {
                    startTime = 300f,
                    endTime = 480f,
                    baseSpawnCount = 40,
                    spawnInterval = 0.8f,
                    eliteChance = 0.45f,
                    bossChance = 0.12f,
                    scaling = new EnemyScaling { healthMultiplier = 5f, damageMultiplier = 3f, speedMultiplier = 1.4f }
                },
                // Wave 6: 8-12 minutes - Challenge Mode
                new WaveConfig
                {
                    startTime = 480f,
                    endTime = 720f,
                    baseSpawnCount = 50,
                    spawnInterval = 0.6f,
                    eliteChance = 0.55f,
                    bossChance = 0.15f,
                    scaling = new EnemyScaling { healthMultiplier = 8f, damageMultiplier = 4f, speedMultiplier = 1.5f }
                },
                // Wave 7: 12+ minutes - Endless Hell
                new WaveConfig
                {
                    startTime = 720f,
                    endTime = float.MaxValue,
                    baseSpawnCount = 60,
                    spawnInterval = 0.5f,
                    eliteChance = 0.65f,
                    bossChance = 0.2f,
                    scaling = new EnemyScaling { healthMultiplier = 12f, damageMultiplier = 5f, speedMultiplier = 1.6f }
                }
            };
        }

        private void Update()
        {
            float gameTime = GameTime;

            ProcessSpawnQueue();

            if (_currentWave != null && gameTime >= _currentWave.endTime)
            {
                if (_currentWaveIndex + 1 < waves.Count)
                {
                    StartWave(_currentWaveIndex + 1);
                }
            }

            if (Time.time >= _nextSpawnTime)
            {
                QueueWaveEnemies();
                _nextSpawnTime = Time.time + _currentWave.spawnInterval;
            }
        }

        private void QueueWaveEnemies()
        {
            if (_enemyManager.ActiveEnemyCount >= maxSimultaneousEnemies)
                return;

            int spawnCount = _currentWave.baseSpawnCount;
            EnemyScaling scaling = CalculateCurrentScaling();

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                EnemyType enemyType = DetermineEnemyType();
                int prefabIndex = GetEnemyPrefabIndex(enemyType);

                _spawnQueue.Enqueue(new SpawnRequest
                {
                    Position = spawnPosition,
                    Scaling = scaling,
                    Type = enemyType,
                    PrefabIndex = prefabIndex
                });
            }
        }

        private void ProcessSpawnQueue()
        {
            int spawned = 0;
            while (_spawnQueue.Count > 0 && spawned < maxSpawnsPerFrame)
            {
                SpawnRequest request = _spawnQueue.Dequeue();
                Enemy enemy = _enemyManager.SpawnEnemy(request.PrefabIndex, request.Position);

                if (enemy)
                {
                    ApplyScaling(enemy, request.Scaling, request.Type);
                }

                spawned++;
            }
        }

        private void StartWave(int waveIndex)
        {
            _currentWaveIndex = waveIndex;
            _currentWave = waves[waveIndex];
            _nextSpawnTime = Time.time;

            Debug.Log($"Wave {CurrentWaveNumber} started!");
        }

        private EnemyType DetermineEnemyType()
        {
            float roll = UnityEngine.Random.value;

            if (roll <= _currentWave.bossChance)
                return EnemyType.Boss;

            if (roll <= _currentWave.bossChance + _currentWave.eliteChance)
                return EnemyType.Elite;

            return EnemyType.Normal;
        }

        private int GetEnemyPrefabIndex(EnemyType type)
        {
            return type switch
            {
                EnemyType.Elite => Mathf.Min(1, _enemyManager.enemyPrefabs.Length - 1),
                EnemyType.Boss => Mathf.Min(2, _enemyManager.enemyPrefabs.Length - 1),
                _ => 0
            };
        }

        private void ApplyScaling(Enemy enemy, EnemyScaling scaling, EnemyType type)
        {
            EnemyData scaledData = enemy.data;

            scaledData.health *= scaling.healthMultiplier;
            scaledData.attackDamage *= scaling.damageMultiplier;
            scaledData.speed *= scaling.speedMultiplier;

            switch (type)
            {
                case EnemyType.Elite:
                    scaledData.health *= 4f;
                    scaledData.attackDamage *= 2.5f;
                    scaledData.speed *= 0.85f;
                    enemy.experienceValue *= 8f;
                    enemy.transform.localScale *= 1.4f;
                    break;

                case EnemyType.Boss:
                    scaledData.health *= 15f;
                    scaledData.attackDamage *= 4f;
                    scaledData.speed *= 0.6f;
                    scaledData.attackRange *= 2f;
                    enemy.experienceValue *= 30f;
                    enemy.transform.localScale *= 2.5f;
                    break;
            }

            enemy.data = scaledData;
            enemy.SetEnemyType(type);
        }

        private EnemyScaling CalculateCurrentScaling()
        {
            float minutesPassed = GameTime / 60f;

            return new EnemyScaling
            {
                healthMultiplier = _currentWave.scaling.healthMultiplier *
                                   (1f + healthScalingPerMinute * minutesPassed),
                damageMultiplier = _currentWave.scaling.damageMultiplier *
                                   (1f + damageScalingPerMinute * minutesPassed),
                speedMultiplier = _currentWave.scaling.speedMultiplier *
                                  (1f + speedScalingPerMinute * minutesPassed)
            };
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Transform playerTransform = _enemyManager.PlayerTransform;
            if (!playerTransform) return Vector3.zero;

            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            return playerTransform.position +
                   new Vector3(randomDirection.x, randomDirection.y, 0) * _enemyManager.spawnRadius;
        }
    }
}