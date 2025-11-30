using System;
using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public AchievementType type;
        public int coinReward;
        public List<AchievementMilestone> milestones;
        public bool isUnlocked;

        public Achievement(string id, string name, string description, AchievementType type, int coinReward)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.coinReward = coinReward;
            this.milestones = new List<AchievementMilestone>();
            this.isUnlocked = false;
        }
    }

    [Serializable]
    public class AchievementMilestone
    {
        public int targetValue;
        public int coinReward;
        public ShopUnlock shopUnlock;
        public bool isCompleted;

        public AchievementMilestone(int targetValue, int coinReward, ShopUnlock unlock = null)
        {
            this.targetValue = targetValue;
            this.coinReward = coinReward;
            this.shopUnlock = unlock;
            this.isCompleted = false;
        }
    }

    public enum AchievementType
    {
        TotalDamageDealt,
        TotalEnemiesKilled,
        TotalHealingReceived,
        TotalCoinsCollected,
        TotalDistanceTraveled,
        SurvivalTime,
        CriticalHits,
        SubclassUnlock,
        LongestKillStreak
    }
}