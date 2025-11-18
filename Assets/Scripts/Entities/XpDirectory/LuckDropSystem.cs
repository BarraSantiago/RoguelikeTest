using Entities.EnemyDirectory;
using UnityEngine;
using PlayerDirectory;

namespace Entities.XpDirectory
{
    public class LuckDropSystem : MonoBehaviour
    {
        [SerializeField] private GameObject goldCoinPrefab;
        [SerializeField] private float luckDropChance = 0.05f; // 5% base

        private Player _player;

        private void Awake()
        {
            _player = FindObjectOfType<Player>();
            Enemy.OnEnemyDamaged += OnEnemyKilled;
        }

        private void OnDestroy()
        {
            Enemy.OnEnemyDamaged -= OnEnemyKilled;
        }

        private void OnEnemyKilled(Enemy enemy, float damage, bool isCritical)
        {
            if (!enemy.IsDead() || !_player) return;

            // Luck increases drop chance
            float totalLuck = _player.playerStats.luck * _player.statsMod.luckMod;
            float finalChance = luckDropChance * totalLuck;

            if (Random.value < finalChance)
            {
                SpawnBonusDrop(enemy.transform.position);
            }
        }

        private void SpawnBonusDrop(Vector3 position)
        {
            // Spawn gold coin or other valuable drops
            if (goldCoinPrefab)
            {
                Instantiate(goldCoinPrefab, position, Quaternion.identity);
            }
        }
    }
}