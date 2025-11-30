using Entities.EnemyDirectory;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI enemyCountText;

        private WaveSystem _waveSystem;
        private EnemyManager _enemyManager;

        private void Start()
        {
            _waveSystem = FindFirstObjectByType<WaveSystem>();
            _enemyManager = FindFirstObjectByType<EnemyManager>();
        }

        private void Update()
        {
            if (!_waveSystem || !_enemyManager) return;

            waveText.text = $"Wave: {_waveSystem.CurrentWaveNumber}";
            timerText.text = $"Time: {FormatTime(_waveSystem.GameTime)}";
            enemyCountText.text = $"Enemies: {_enemyManager.ActiveEnemyCount}";
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}