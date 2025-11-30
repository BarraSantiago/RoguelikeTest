using System;
using UnityEngine;

namespace Achievements
{
    [Serializable]
    public class PersistentStats
    {
        public float totalDamageDealt;
        public int totalEnemiesKilled;
        public float totalHealingReceived;
        public int totalCoinsCollected;
        public float totalDistanceTraveled;
        public float totalSurvivalTime;
        public int totalCriticalHits;
        public int totalGamesPlayed;
        public float longestSurvivalTime;
        public int longestKillStreak;
        public int totalRevivals;
        public int totalPowerUpsCollected;
        public int totalProjectilesFired;

        public void AddSessionStats(PlayerDirectory.GameSessionStats sessionStats)
        {
            totalDamageDealt += sessionStats.totalDamageDealt;
            totalEnemiesKilled += sessionStats.totalKills;
            totalHealingReceived += sessionStats.totalHealingReceived;
            totalCoinsCollected += sessionStats.totalCoinsCollected;
            totalDistanceTraveled += sessionStats.totalDistanceTraveled;
            totalSurvivalTime += sessionStats.survivalTime;
            totalCriticalHits += sessionStats.criticalHits;
            totalRevivals += sessionStats.revivals;
            totalPowerUpsCollected += sessionStats.powerUpsCollected;
            totalProjectilesFired += sessionStats.projectilesFired;
            totalGamesPlayed++;

            if (sessionStats.survivalTime > longestSurvivalTime)
                longestSurvivalTime = sessionStats.survivalTime;

            if (sessionStats.longestKillStreak > longestKillStreak)
                longestKillStreak = (int)sessionStats.longestKillStreak;
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("PersistentStats", json);
            PlayerPrefs.Save();
        }

        public static PersistentStats Load()
        {
            if (PlayerPrefs.HasKey("PersistentStats"))
            {
                string json = PlayerPrefs.GetString("PersistentStats");
                return JsonUtility.FromJson<PersistentStats>(json);
            }

            return new PersistentStats();
        }
    }
}