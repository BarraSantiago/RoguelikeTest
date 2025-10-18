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
        [Range(0,1)]public float eliteChance;
        [Range(0,1)]public float bossChance;
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
        [SerializeField] private int maxSimultaneousEnemies = 50;

        [Header("Scaling Settings")] 
        [SerializeField] private float healthScalingPerMinute = 0.1f;
        [SerializeField] private float damageScalingPerMinute = 0.08f;
        [SerializeField] private float speedScalingPerMinute = 0.05f;

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
                // Wave 1: 0-1 minute
                new WaveConfig
                {
                    startTime = 0f,
                    endTime = 60f,
                    baseSpawnCount = 5,
                    spawnInterval = 2f,
                    eliteChance = 0f,
                    bossChance = 0f,
                    scaling = new EnemyScaling { healthMultiplier = 1f, damageMultiplier = 1f, speedMultiplier = 1f }
                },
                // Wave 2: 1-3 minutes
                new WaveConfig
                {
                    startTime = 60f,
                    endTime = 180f,
                    baseSpawnCount = 8,
                    spawnInterval = 1.5f,
                    eliteChance = 0.1f,
                    bossChance = 0f,
                    scaling = new EnemyScaling
                        { healthMultiplier = 1.5f, damageMultiplier = 1.3f, speedMultiplier = 1.1f }
                },
                // Wave 3: 3-5 minutes
                new WaveConfig
                {
                    startTime = 180f,
                    endTime = 300f,
                    baseSpawnCount = 12,
                    spawnInterval = 1.2f,
                    eliteChance = 0.15f,
                    bossChance = 0.05f,
                    scaling = new EnemyScaling
                        { healthMultiplier = 2f, damageMultiplier = 1.6f, speedMultiplier = 1.2f }
                },
                // Wave 4: 5-8 minutes
                new WaveConfig
                {
                    startTime = 300f,
                    endTime = 480f,
                    baseSpawnCount = 15,
                    spawnInterval = 1f,
                    eliteChance = 0.2f,
                    bossChance = 0.1f,
                    scaling = new EnemyScaling { healthMultiplier = 3f, damageMultiplier = 2f, speedMultiplier = 1.3f }
                },
                // Wave 5: 8+ minutes (endless)
                new WaveConfig
                {
                    startTime = 480f,
                    endTime = float.MaxValue,
                    baseSpawnCount = 20,
                    spawnInterval = 0.8f,
                    eliteChance = 0.25f,
                    bossChance = 0.15f,
                    scaling = new EnemyScaling
                        { healthMultiplier = 4f, damageMultiplier = 2.5f, speedMultiplier = 1.4f }
                }
            };
        }

        private void Update()
        {
            float gameTime = GameTime;

            // Check if we should transition to next wave
            if (_currentWave != null && gameTime >= _currentWave.endTime)
            {
                if (_currentWaveIndex + 1 < waves.Count)
                {
                    StartWave(_currentWaveIndex + 1);
                }
            }

            // Spawn enemies
            if (Time.time >= _nextSpawnTime)
            {
                SpawnWaveEnemies();
                _nextSpawnTime = Time.time + _currentWave.spawnInterval;
            }
        }

        private void StartWave(int waveIndex)
        {
            _currentWaveIndex = waveIndex;
            _currentWave = waves[waveIndex];
            _nextSpawnTime = Time.time;

            Debug.Log($"Wave {CurrentWaveNumber} started!");
        }

        private void SpawnWaveEnemies()
        {
            if (_enemyManager.ActiveEnemyCount >= maxSimultaneousEnemies)
                return;

            int spawnCount = _currentWave.baseSpawnCount;
            EnemyScaling scaling = CalculateCurrentScaling();

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnScaledEnemy(scaling);
            }
        }

        private void SpawnScaledEnemy(EnemyScaling scaling)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            EnemyType enemyType = DetermineEnemyType();
            int enemyPrefabIndex = GetEnemyPrefabIndex(enemyType);

            Enemy enemy = _enemyManager.SpawnEnemy(enemyPrefabIndex, spawnPosition);
            if (enemy == null) return;

            ApplyScaling(enemy, scaling, enemyType);
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
            // Assuming enemy prefabs are ordered: Normal, Elite, Boss
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

            // Apply wave scaling
            scaledData.health *= scaling.healthMultiplier;
            scaledData.attackDamage *= scaling.damageMultiplier;
            scaledData.speed *= scaling.speedMultiplier;

            // Apply type multipliers
            switch (type)
            {
                case EnemyType.Elite:
                    scaledData.health *= 3f;
                    scaledData.attackDamage *= 2f;
                    scaledData.speed *= 0.8f;
                    enemy.experienceValue *= 5f;
                    enemy.transform.localScale *= 1.3f;
                    break;

                case EnemyType.Boss:
                    scaledData.health *= 10f;
                    scaledData.attackDamage *= 3f;
                    scaledData.speed *= 0.6f;
                    scaledData.attackRange *= 1.5f;
                    enemy.experienceValue *= 20f;
                    enemy.transform.localScale *= 2f;
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