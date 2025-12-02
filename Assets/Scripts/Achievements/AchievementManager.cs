using System.Collections.Generic;
using System.Linq;
using PlayerDirectory;
using UnityEngine;
using UnityEngine.Events;

namespace Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Events")] 
        public UnityEvent<Achievement> onAchievementUnlocked;
        public UnityEvent<int> onCoinsEarned;
        public UnityEvent onShopItemUnlocked;

        private PersistentStats _persistentStats;
        private Dictionary<string, Achievement> _achievements;
        private int _totalCoins;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _persistentStats = PersistentStats.Load();
            _totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            _achievements = new Dictionary<string, Achievement>();
            onShopItemUnlocked = new UnityEvent();

            InitializeAchievements();
            LoadAchievementProgress();
        }

        private void InitializeAchievements()
        {
            // Damage achievements
            Achievement damageAchievement = new Achievement(
                "total_damage",
                "Damage Dealer",
                "Deal massive damage to enemies",
                AchievementType.TotalDamageDealt,
                0
            );
            damageAchievement.milestones.Add(new AchievementMilestone(1000, 50));
            damageAchievement.milestones.Add(new AchievementMilestone(10000, 100));
            damageAchievement.milestones.Add(new AchievementMilestone(50000, 250));
            damageAchievement.milestones.Add(new AchievementMilestone(100000, 500));
            _achievements["total_damage"] = damageAchievement;

            // Kill achievements
            Achievement killAchievement = new Achievement(
                "total_kills",
                "Executioner",
                "Eliminate enemies",
                AchievementType.TotalEnemiesKilled,
                0
            );

            killAchievement.milestones.Add(new AchievementMilestone(100, 25,
                new ShopUnlock("reroll_1", ShopUnlockType.StatBonus, "reroll", 50)));
            killAchievement.milestones.Add(new AchievementMilestone(500, 75,
                new ShopUnlock("reroll_2", ShopUnlockType.StatBonus, "reroll", 100)));
            killAchievement.milestones.Add(new AchievementMilestone(1000, 150));
            killAchievement.milestones.Add(new AchievementMilestone(5000, 500,
                new ShopUnlock("reroll_3", ShopUnlockType.StatBonus, "reroll", 200)));
            _achievements["total_kills"] = killAchievement;

            // Healing achievements
            Achievement healingAchievement = new Achievement(
                "total_healing",
                "Regenerator",
                "Recover health throughout your runs",
                AchievementType.TotalHealingReceived,
                0
            );
            healingAchievement.milestones.Add(new AchievementMilestone(500, 30));
            healingAchievement.milestones.Add(new AchievementMilestone(2000, 75,
                new ShopUnlock("health_regen_1", ShopUnlockType.StatBonus, "health_regen", 75)));
            healingAchievement.milestones.Add(new AchievementMilestone(5000, 150));
            _achievements["total_healing"] = healingAchievement;

            // Coin achievements
            Achievement coinAchievement = new Achievement(
                "total_coins",
                "Treasure Hunter",
                "Collect coins during runs",
                AchievementType.TotalCoinsCollected,
                0
            );
            coinAchievement.milestones.Add(new AchievementMilestone(500, 50,
                new ShopUnlock("growth_1", ShopUnlockType.StatBonus, "growth", 100)));
            coinAchievement.milestones.Add(new AchievementMilestone(2000, 150));
            coinAchievement.milestones.Add(new AchievementMilestone(10000, 500,
                new ShopUnlock("growth_2", ShopUnlockType.StatBonus, "growth", 250)));
            _achievements["total_coins"] = coinAchievement;

            // Distance achievements
            Achievement distanceAchievement = new Achievement(
                "total_distance",
                "Marathon Runner",
                "Travel vast distances",
                AchievementType.TotalDistanceTraveled,
                0
            );
            distanceAchievement.milestones.Add(new AchievementMilestone(1000, 40));
            distanceAchievement.milestones.Add(new AchievementMilestone(5000, 100,
                new ShopUnlock("pickup_radius_1", ShopUnlockType.StatBonus, "pickup_radius", 75)));
            distanceAchievement.milestones.Add(new AchievementMilestone(20000, 300));
            _achievements["total_distance"] = distanceAchievement;

            // Survival achievements
            Achievement survivalAchievement = new Achievement(
                "survival_time",
                "Survivor",
                "Survive for extended periods",
                AchievementType.SurvivalTime,
                0
            );
            survivalAchievement.milestones.Add(new AchievementMilestone(600, 50)); // 10 min
            survivalAchievement.milestones.Add(new AchievementMilestone(1800, 150,
                new ShopUnlock("skip_1", ShopUnlockType.StatBonus, "skip", 100))); // 30 min
            survivalAchievement.milestones.Add(new AchievementMilestone(3600, 400)); // 60 min
            _achievements["survival_time"] = survivalAchievement;

            // Critical hit achievements
            Achievement critAchievement = new Achievement(
                "critical_hits",
                "Precision Striker",
                "Land critical hits",
                AchievementType.CriticalHits,
                0
            );
            critAchievement.milestones.Add(new AchievementMilestone(100, 30));
            critAchievement.milestones.Add(new AchievementMilestone(500, 100,
                new ShopUnlock("luck_1", ShopUnlockType.StatBonus, "luck", 125)));
            critAchievement.milestones.Add(new AchievementMilestone(2000, 300));
            _achievements["critical_hits"] = critAchievement;

            // Kill streak achievements
            Achievement streakAchievement = new Achievement(
                "kill_streak",
                "Unstoppable",
                "Achieve high kill streaks",
                AchievementType.LongestKillStreak,
                0
            );
            streakAchievement.milestones.Add(new AchievementMilestone(50, 50));
            streakAchievement.milestones.Add(new AchievementMilestone(150, 150,
                new ShopUnlock("banish_1", ShopUnlockType.StatBonus, "banish", 150)));
            streakAchievement.milestones.Add(new AchievementMilestone(300, 400));
            _achievements["kill_streak"] = streakAchievement;

            // Subclass unlock achievements
            AddSubclassAchievements();
        }

        private void AddSubclassAchievements()
        {
            string[] subclasses = { "Berserker", "Paladin", "Reaver", "Sniper", "Hunter", "Trapper" };

            foreach (string subclass in subclasses)
            {
                Achievement achievement = new Achievement(
                    $"unlock_{subclass.ToLower()}",
                    $"Unlock {subclass}",
                    $"Complete the conditions to unlock {subclass}",
                    AchievementType.SubclassUnlock,
                    100
                );
                achievement.milestones.Add(new AchievementMilestone(1, 0,
                    new ShopUnlock($"subclass_{subclass.ToLower()}", ShopUnlockType.Subclass, subclass, 500)));
                _achievements[$"unlock_{subclass.ToLower()}"] = achievement;
            }
        }

        public void OnSessionEnd(GameSessionStats sessionStats)
        {
            _persistentStats.AddSessionStats(sessionStats);
            _persistentStats.Save();
        
            // Add session coins to total
            AddCoins(sessionStats.totalCoinsCollected);
        
            CheckAchievementProgress();
        }

        private void AddCoins(int totalCoins)
        {
            _totalCoins += totalCoins;
            PlayerPrefs.SetInt("TotalCoins", _totalCoins);
            PlayerPrefs.Save();
            onCoinsEarned?.Invoke(totalCoins);
        }

        public void DebugUnlockAllAchievements()
        {
            foreach (Achievement achievement in _achievements.Values)
            {
                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    if (!milestone.isCompleted)
                    {
                        milestone.isCompleted = true;
        
                        if (milestone.shopUnlock != null)
                        {
                            string key = $"ShopUnlock_{milestone.shopUnlock.id}";
                            PlayerPrefs.SetInt(key, 1);
                        }
                    }
                }
        
                if (!achievement.isUnlocked)
                {
                    achievement.isUnlocked = true;
                    _totalCoins += achievement.coinReward;
                }
            }
        
            PlayerPrefs.SetInt("TotalCoins", _totalCoins);
            SaveAchievementProgress();
            PlayerPrefs.Save();
        
            onShopItemUnlocked?.Invoke();
            Debug.Log("[DEBUG] All achievements unlocked (without Google Play sync)");
        }
        
        public void DebugClearAllProgress()
        {
            // Clear all achievement progress
            foreach (string key in _achievements.Keys)
            {
                PlayerPrefs.DeleteKey($"Achievement_{key}");
            }
        
            // Clear all shop unlocks
            List<string> keysToDelete = new List<string>();
            for (int i = 0; i < PlayerPrefs.GetInt("ShopUnlockCount", 0); i++)
            {
                string key = $"ShopUnlock_{i}";
                if (PlayerPrefs.HasKey(key))
                {
                    keysToDelete.Add(key);
                }
            }
        
            foreach (string key in keysToDelete)
            {
                PlayerPrefs.DeleteKey(key);
            }
        
            // Reset coins
            _totalCoins = 0;
            PlayerPrefs.SetInt("TotalCoins", 0);
        
            // Reinitialize achievements
            foreach (Achievement achievement in _achievements.Values)
            {
                achievement.isUnlocked = false;
                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    milestone.isCompleted = false;
                }
            }
        
            PlayerPrefs.Save();
            Debug.Log("[DEBUG] All progress cleared");
        }
        
        private void CheckAchievementProgress()
        {
            foreach (Achievement achievement in _achievements.Values)
            {
                float currentValue = GetCurrentValueForType(achievement.type);

                foreach (AchievementMilestone milestone in achievement.milestones)
                {
                    if (!milestone.isCompleted && currentValue >= milestone.targetValue)
                    {
                        milestone.isCompleted = true;
                        AwardCoins(milestone.coinReward);

                        if (milestone.shopUnlock != null)
                        {
                            UnlockShopItem(milestone.shopUnlock);
                        }

                        // Sync with Google Play Games
                        SyncMilestoneToGooglePlay(achievement.id, milestone);

                        Debug.Log($"Milestone completed: {achievement.name} - {milestone.targetValue}");
                    }
                }

                if (!achievement.isUnlocked && achievement.milestones.All(m => m.isCompleted))
                {
                    achievement.isUnlocked = true;
                    AwardCoins(achievement.coinReward);

                    // Sync with Google Play Games
                    SyncAchievementToGooglePlay(achievement.id);

                    onAchievementUnlocked?.Invoke(achievement);
                    Debug.Log($"Achievement unlocked: {achievement.name}");
                }
            }

            SaveAchievementProgress();
        }

        private void SyncMilestoneToGooglePlay(string achievementId, AchievementMilestone milestone)
        {
            string gpgsId = GetGPGSIdForAchievement(achievementId, milestone);
            if (!string.IsNullOrEmpty(gpgsId))
            {
                GooglePlay.Achievement.UnlockAchievement(gpgsId);
            }
        }

        private void SyncAchievementToGooglePlay(string achievementId)
        {
            string gpgsId = GetGPGSIdForAchievement(achievementId, null);
            if (!string.IsNullOrEmpty(gpgsId))
            {
                GooglePlay.Achievement.UnlockAchievement(gpgsId);
            }
        }

        private string GetGPGSIdForAchievement(string achievementId, AchievementMilestone milestone)
        {
            // Map internal achievement IDs to Google Play Games IDs
            return (achievementId, milestone?.targetValue) switch
            {
                ("total_damage", 1000) => GPGSIds.achievement_damage_1k,
                ("total_damage", 10000) => GPGSIds.achievement_damage_10k,
                ("total_damage", 50000) => GPGSIds.achievement_damage_50k,
                ("total_damage", 100000) => GPGSIds.achievement_damage_100k,

                ("total_kills", 100) => GPGSIds.achievement_kills_100,
                ("total_kills", 500) => GPGSIds.achievement_kills_500,
                ("total_kills", 1000) => GPGSIds.achievement_kills_1k,
                ("total_kills", 5000) => GPGSIds.achievement_kills_5k,

                ("total_healing", 500) => GPGSIds.achievement_healing_500,
                ("total_healing", 2000) => GPGSIds.achievement_healing_2k,
                ("total_healing", 5000) => GPGSIds.achievement_healing_5k,

                ("total_coins", 500) => GPGSIds.achievement_coins_500,
                ("total_coins", 2000) => GPGSIds.achievement_coins_2k,
                ("total_coins", 10000) => GPGSIds.achievement_coins_10k,

                ("total_distance", 1000) => GPGSIds.achievement_distance_1k,
                ("total_distance", 5000) => GPGSIds.achievement_distance_5k,
                ("total_distance", 20000) => GPGSIds.achievement_distance_20k,

                ("survival_time", 360) => GPGSIds.achievement_survival_6min,
                ("survival_time", 600) => GPGSIds.achievement_survival_10min,
                ("survival_time", 1800) => GPGSIds.achievement_survival_30min,

                ("critical_hits", 100) => GPGSIds.achievement_crits_100,
                ("critical_hits", 500) => GPGSIds.achievement_crits_500,
                ("critical_hits", 2000) => GPGSIds.achievement_crits_2k,

                ("kill_streak", 50) => GPGSIds.achievement_streak_50,
                ("kill_streak", 150) => GPGSIds.achievement_streak_150,
                ("kill_streak", 300) => GPGSIds.achievement_streak_300,

                ("unlock_berserker", 1) => GPGSIds.achievement_unlock_berserker,
                ("unlock_paladin", 1) => GPGSIds.achievement_unlock_paladin,
                ("unlock_reaver", 1) => GPGSIds.achievement_unlock_reaver,
                ("unlock_sniper", 1) => GPGSIds.achievement_unlock_sniper,
                ("unlock_hunter", 1) => GPGSIds.achievement_unlock_hunter,
                ("unlock_trapper", 1) => GPGSIds.achievement_unlock_trapper,

                _ => string.Empty
            };
        }

        private float GetCurrentValueForType(AchievementType type)
        {
            return type switch
            {
                AchievementType.TotalDamageDealt => _persistentStats.totalDamageDealt,
                AchievementType.TotalEnemiesKilled => _persistentStats.totalEnemiesKilled,
                AchievementType.TotalHealingReceived => _persistentStats.totalHealingReceived,
                AchievementType.TotalCoinsCollected => _persistentStats.totalCoinsCollected,
                AchievementType.TotalDistanceTraveled => _persistentStats.totalDistanceTraveled,
                AchievementType.SurvivalTime => _persistentStats.totalSurvivalTime,
                AchievementType.CriticalHits => _persistentStats.totalCriticalHits,
                AchievementType.LongestKillStreak => _persistentStats.longestKillStreak,
                _ => 0
            };
        }

        public void OnSubclassConditionMet(string subclassName)
        {
            string achievementId = $"unlock_{subclassName.ToLower()}";
            if (_achievements.TryGetValue(achievementId, out Achievement achievement))
            {
                achievement.milestones[0].isCompleted = true;
                CheckAchievementProgress();
            }
        }

        private void AwardCoins(int amount)
        {
            _totalCoins += amount;
            PlayerPrefs.SetInt("TotalCoins", _totalCoins);
            PlayerPrefs.Save();
            onCoinsEarned?.Invoke(amount);
        }

        private void UnlockShopItem(ShopUnlock unlock)
        {
            string key = $"ShopUnlock_{unlock.id}";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"Unlocked shop item: {unlock.id}");
            
            onShopItemUnlocked?.Invoke();
        }

        private void SaveAchievementProgress()
        {
            foreach (KeyValuePair<string, Achievement> kvp in _achievements)
            {
                string json = JsonUtility.ToJson(kvp.Value);
                PlayerPrefs.SetString($"Achievement_{kvp.Key}", json);
            }

            PlayerPrefs.Save();
        }

        private void LoadAchievementProgress()
        {
            foreach (KeyValuePair<string, Achievement> kvp in _achievements)
            {
                string key = $"Achievement_{kvp.Key}";
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    Achievement loaded = JsonUtility.FromJson<Achievement>(json);
                    kvp.Value.isUnlocked = loaded.isUnlocked;
                    for (int i = 0; i < loaded.milestones.Count && i < kvp.Value.milestones.Count; i++)
                    {
                        kvp.Value.milestones[i].isCompleted = loaded.milestones[i].isCompleted;
                    }
                }
            }
        }

        public int GetTotalCoins() => _totalCoins;
        public PersistentStats GetPersistentStats() => _persistentStats;
        public Dictionary<string, Achievement> GetAllAchievements() => _achievements;

        public bool SpendCoins(int amount)
        {
            if (_totalCoins >= amount)
            {
                _totalCoins -= amount;
                PlayerPrefs.SetInt("TotalCoins", _totalCoins);
                PlayerPrefs.Save();
                return true;
            }

            return false;
        }
    }
}