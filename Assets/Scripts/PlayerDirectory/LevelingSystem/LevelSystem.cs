using System;
using UnityEngine;

namespace PlayerDirectory.LevelingSystem
{
    [Serializable]
    public class LevelSystem
    {
        [SerializeField] private float baseExperience = 100f;
        [SerializeField] private float exponentFactor = 1.1f;
        [SerializeField] private float linearFactor = 10f;

        public int CurrentLevel { get; private set; } = 1;
        public float CurrentExperience { get; private set; } = 0f;
        public float ExperienceToNextLevel { get; private set; }

        public LevelSystem()
        {
            ExperienceToNextLevel = CalculateExperienceForLevel(CurrentLevel);
        }

        public bool AddExperience(float amount)
        {
            CurrentExperience += amount;

            if (CurrentExperience >= ExperienceToNextLevel)
            {
                LevelUp();
                return true;
            }

            return false;
        }

        private void LevelUp()
        {
            CurrentLevel++;
            CurrentExperience -= ExperienceToNextLevel;
            ExperienceToNextLevel = CalculateExperienceForLevel(CurrentLevel);
        }

        private float CalculateExperienceForLevel(int level)
        {
            // Formula: baseXP * (level^exponent + level * linear)
            return baseExperience * (Mathf.Pow(level, exponentFactor) + level * linearFactor);
        }
    }
}