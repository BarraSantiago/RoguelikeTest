using System.Collections.Generic;
using Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Menus.Achievements
{
    public class AchievementsMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform achievementsContainer;
        [SerializeField] private GameObject achievementCardPrefab;
        [SerializeField] private TextMeshProUGUI totalProgressText;
        [SerializeField] private Button closeButton;

        [Header("Filters")]
        [SerializeField] private Toggle showCompletedToggle;
        [SerializeField] private Toggle showInProgressToggle;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            showCompletedToggle.onValueChanged.AddListener(_ => RefreshDisplay());
            showInProgressToggle.onValueChanged.AddListener(_ => RefreshDisplay());
        }

        private void OnEnable()
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }

            Dictionary<string, Achievement> achievements = AchievementManager.Instance.GetAllAchievements();
            PersistentStats stats = AchievementManager.Instance.GetPersistentStats();

            int completed = 0;
            int total = 0;

            foreach (Achievement achievement in achievements.Values)
            {
                total++;
                if (achievement.isUnlocked) completed++;

                bool shouldShow = ShouldShowAchievement(achievement);
                if (!shouldShow) continue;

                GameObject card = Instantiate(achievementCardPrefab, achievementsContainer);
                AchievementCard cardComponent = card.GetComponent<AchievementCard>();

                float currentValue = GetCurrentValueForType(achievement.type, stats);
                cardComponent.Setup(achievement, currentValue);
            }

            totalProgressText.text = $"Completed: {completed}/{total}";
        }

        private bool ShouldShowAchievement(Achievement achievement)
        {
            if (showCompletedToggle.isOn && achievement.isUnlocked) return true;
            if (showInProgressToggle.isOn && !achievement.isUnlocked) return true;
            return false;
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
    }
}