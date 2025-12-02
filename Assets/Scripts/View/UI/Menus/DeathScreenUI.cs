using System.Collections.Generic;
using System.Linq;
using Achievements;
using ClassesDirectory;
using ClassesDirectory.Evolution;
using GooglePlay;
using PlayerDirectory;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using View.UI.Menus.Achievements;
using Achievement = Achievements.Achievement;

namespace View.UI.Menus
{
    public class DeathScreenUI : MonoBehaviour
    {
        [Header("Screen References")]
        [SerializeField] private GameObject statsScreen;
        [SerializeField] private GameObject progressScreen;
        [SerializeField] private GameObject achievementsScreen;

        [Header("Stats Screen Elements")]
        [SerializeField] private TextMeshProUGUI survivalTimeText;
        [SerializeField] private TextMeshProUGUI totalKillsText;
        [SerializeField] private TextMeshProUGUI totalDamageText;
        [SerializeField] private TextMeshProUGUI damageTakenText;
        [SerializeField] private TextMeshProUGUI healingReceivedText;
        [SerializeField] private TextMeshProUGUI coinsCollectedText;
        [SerializeField] private TextMeshProUGUI highestLevelText;
        [SerializeField] private TextMeshProUGUI criticalHitsText;
        [SerializeField] private TextMeshProUGUI killStreakText;
        [SerializeField] private TextMeshProUGUI revivalsUsedText;
        [SerializeField] private Button statsNextButton;

        [Header("Progress Screen Elements")]
        [SerializeField] private Transform progressContainer;
        [SerializeField] private GameObject progressItemPrefab;
        [SerializeField] private Button progressNextButton;

        [Header("Achievements Screen Elements")]
        [SerializeField] private Transform achievementsContainer;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private TextMeshProUGUI totalCoinsEarnedText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private GameSessionStats _sessionStats;
        private SubclassEvolutionManager _evolutionManager;
        private List<string> _unlockedAchievements = new();
        private int _coinsEarned;

        private void Awake()
        {
            statsNextButton.onClick.AddListener(ShowProgressScreen);
            progressNextButton.onClick.AddListener(ShowAchievementsScreen);
            restartButton.onClick.AddListener(RestartGame);
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        public void ShowDeathScreen(GameSessionStats stats, SubclassEvolutionManager evolutionManager)
        {
            _sessionStats = stats;
            _evolutionManager = evolutionManager;

            gameObject.SetActive(true);
            ShowStatsScreen();
            GooglePlayManager.Instance.SubmitKillsScore(stats.totalKills);
        }

        private void ShowStatsScreen()
        {
            statsScreen.SetActive(true);
            progressScreen.SetActive(false);
            achievementsScreen.SetActive(false);

            survivalTimeText.text = $"Survival Time: {FormatTime(_sessionStats.survivalTime)}";
            totalKillsText.text = $"Total Kills: {_sessionStats.totalKills}";
            totalDamageText.text = $"Total Damage: {_sessionStats.totalDamageDealt:F0}";
            damageTakenText.text = $"Damage Taken: {_sessionStats.totalDamageTaken:F0}";
            healingReceivedText.text = $"Healing Received: {_sessionStats.totalHealingReceived:F0}";
            coinsCollectedText.text = $"Coins Collected: {_sessionStats.totalCoinsCollected}";
            highestLevelText.text = $"Highest Level: {_sessionStats.highestLevel}";
            criticalHitsText.text = $"Critical Hits: {_sessionStats.criticalHits}";
            killStreakText.text = $"Longest Kill Streak: {_sessionStats.longestKillStreak:F0}";
            revivalsUsedText.text = $"Revivals Used: {_sessionStats.revivals}";
        }

        private void ShowProgressScreen()
        {
            statsScreen.SetActive(false);
            progressScreen.SetActive(true);
            achievementsScreen.SetActive(false);

            PopulateProgressList();
        }

        private void ShowAchievementsScreen()
        {
            statsScreen.SetActive(false);
            progressScreen.SetActive(false);
            achievementsScreen.SetActive(true);

            PopulateAchievementsList();
        }

        private void PopulateProgressList()
        {
            foreach (Transform child in progressContainer)
            {
                Destroy(child.gameObject);
            }

            Player player = FindFirstObjectByType<Player>();
            GameSessionStats gameStats = _evolutionManager.GetGameStats();
            Dictionary<string, UnlockCondition> conditions = GetUnlockConditions(player);

            foreach (KeyValuePair<string, UnlockCondition> kvp in conditions)
            {
                GameObject item = Instantiate(progressItemPrefab, progressContainer);
                SubclassProgressItem progressItem = item.GetComponent<SubclassProgressItem>();

                float progress = CalculateProgress(kvp.Value, player, gameStats);
                bool isUnlocked = IsSubclassUnlocked(kvp.Key);

                progressItem.Setup(kvp.Key, kvp.Value.description, progress, isUnlocked);
            }
        }

        private void PopulateAchievementsList()
        {
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }
        
            Dictionary<string, Achievement> allAchievements = AchievementManager.Instance.GetAllAchievements();
            PersistentStats stats = AchievementManager.Instance.GetPersistentStats();
        
            foreach (Achievement achievement in allAchievements.Values)
            {
                float currentValue = GetCurrentValueForType(achievement.type, stats);
                float sessionValue = GetSessionValueForType(achievement.type);
                
                // Skip if no progress was made this session
                if (sessionValue <= 0) continue;
                
                // Check if any milestone was completed or progressed this session
                bool hasProgress = false;
                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    float previousValue = currentValue - sessionValue;
                    if ((previousValue < milestone.targetValue && currentValue >= milestone.targetValue) || 
                        (!milestone.isCompleted && currentValue > previousValue))
                    {
                        hasProgress = true;
                        break;
                    }
                }
                
                if (!hasProgress) continue;
        
                GameObject item = Instantiate(achievementItemPrefab, achievementsContainer);
                AchievementProgressItem achievementItem = item.GetComponent<AchievementProgressItem>();
                achievementItem.Setup(achievement, currentValue, sessionValue);
            }
        
            totalCoinsEarnedText.text = $"Total Coins: {AchievementManager.Instance.GetTotalCoins()}";
        }
        
        private float GetSessionValueForType(AchievementType type)
        {
            return type switch
            {
                AchievementType.TotalDamageDealt => _sessionStats.totalDamageDealt,
                AchievementType.TotalEnemiesKilled => _sessionStats.totalKills,
                AchievementType.TotalHealingReceived => _sessionStats.totalHealingReceived,
                AchievementType.TotalCoinsCollected => _sessionStats.totalCoinsCollected,
                AchievementType.TotalDistanceTraveled => _sessionStats.totalDistanceTraveled,
                AchievementType.SurvivalTime => _sessionStats.survivalTime,
                AchievementType.CriticalHits => _sessionStats.criticalHits,
                AchievementType.LongestKillStreak => _sessionStats.longestKillStreak,
                _ => 0
            };
        }

        private float GetCurrentValueForType(AchievementType type, PersistentStats stats)
        {
            return type switch
            {
                AchievementType.TotalDamageDealt => stats.totalDamageDealt,
                AchievementType.TotalEnemiesKilled => stats.totalEnemiesKilled,
                AchievementType.TotalHealingReceived => stats.totalHealingReceived,
                AchievementType.TotalCoinsCollected => stats.totalCoinsCollected,
                AchievementType.TotalDistanceTraveled => stats.totalDistanceTraveled,
                AchievementType.SurvivalTime => stats.totalSurvivalTime,
                AchievementType.CriticalHits => stats.totalCriticalHits,
                AchievementType.LongestKillStreak => stats.longestKillStreak,
                _ => 0
            };
        }

        private Dictionary<string, UnlockCondition> GetUnlockConditions(Player player)
        {
            Dictionary<string, UnlockCondition> conditions = new Dictionary<string, UnlockCondition>();

            if (player.GetPlayerClassType() == Classes.Warrior)
            {
                conditions["Berserker"] = new UnlockCondition
                {
                    description = "Kill 150 enemies in 90 seconds",
                    condition = (_, stats) => stats.enemiesKilledInWindow >= 150 &&
                                              (Time.time - stats.windowStartTime) <= 90f
                };

                conditions["Paladin"] = new UnlockCondition
                {
                    description = "Survive 4 minutes without dying",
                    condition = (_, stats) => stats.timeSinceLastDeath >= 240f
                };

                conditions["Reaver"] = new UnlockCondition
                {
                    description = "Heal 300 HP total",
                    condition = (_, stats) => stats.totalHealingReceived >= 300f
                };
            }
            else if (player.GetPlayerClassType() == Classes.Ranger)
            {
                conditions["Sniper"] = new UnlockCondition
                {
                    description = "Hit 100 enemies from max range",
                    condition = (_, stats) => stats.maxRangeHits >= 100
                };

                conditions["Hunter"] = new UnlockCondition
                {
                    description = "Get 50 weak point kills",
                    condition = (_, stats) => stats.weakPointKills >= 50
                };

                conditions["Trapper"] = new UnlockCondition
                {
                    description = "Immobilize or slow 80 enemies",
                    condition = (_, stats) => stats.enemiesSlowedOrImmobilized >= 80
                };
            }

            return conditions;
        }

        private float CalculateProgress(UnlockCondition condition, Player player, GameSessionStats stats)
        {
            string desc = condition.description.ToLower();

            if (desc.Contains("150 enemies"))
                return Mathf.Clamp01(stats.enemiesKilledInWindow / 150f);
            if (desc.Contains("4 minutes"))
                return Mathf.Clamp01(stats.timeSinceLastDeath / 240f);
            if (desc.Contains("300 hp"))
                return Mathf.Clamp01(stats.totalHealingReceived / 300f);
            if (desc.Contains("100 enemies from max range"))
                return Mathf.Clamp01(stats.maxRangeHits / 100f);
            if (desc.Contains("50 weak point"))
                return Mathf.Clamp01(stats.weakPointKills / 50f);
            if (desc.Contains("80 enemies"))
                return Mathf.Clamp01(stats.enemiesSlowedOrImmobilized / 80f);

            return 0f;
        }

        private bool IsSubclassUnlocked(string subclass)
        {
            return PlayerPrefs.GetInt($"Unlocked_{subclass}", 0) == 1;
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        private void RestartGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }
    }
}