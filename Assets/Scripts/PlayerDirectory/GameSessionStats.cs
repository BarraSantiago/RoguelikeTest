using System;
using UnityEngine;

namespace PlayerDirectory
{
    [Serializable]
    public class GameSessionStats
    {
        // Session stats (for death screen)
        public float survivalTime;
        public int totalKills;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public float totalHealingReceived;
        public int totalCoinsCollected;
        public int totalExperienceGained;
        public int highestLevel;
        public int criticalHits;
        public float longestKillStreak;
        public int revivals;
        public int powerUpsCollected;
        public float totalDistanceTraveled;
        public int projectilesFired;

        // Evolution tracking stats (for unlock conditions)
        public int enemiesKilledInWindow;
        public float windowStartTime;
        public int maxRangeHits;
        public int weakPointKills;
        public int enemiesSlowedOrImmobilized;
        public float timeSinceLastDeath;
        public int killStreak;
        public float lastKillTime;

        public void Reset()
        {
            survivalTime = 0;
            totalKills = 0;
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            totalHealingReceived = 0;
            totalCoinsCollected = 0;
            totalExperienceGained = 0;
            highestLevel = 1;
            criticalHits = 0;
            longestKillStreak = 0;
            revivals = 0;
            powerUpsCollected = 0;
            totalDistanceTraveled = 0;
            projectilesFired = 0;

            enemiesKilledInWindow = 0;
            windowStartTime = 0;
            maxRangeHits = 0;
            weakPointKills = 0;
            enemiesSlowedOrImmobilized = 0;
            timeSinceLastDeath = 0;
            killStreak = 0;
            lastKillTime = 0;
        }
    }
}