using System;

namespace PlayerDirectory
{
    [Serializable]
    public class BaseStatsMod
    {
        public float damageMod = 1;
        public float attackSpeedMod = 1;
        public float projectileSpeedMod = 1;
        public float rangeMod = 1;
        public float criticalChanceMod = 1;
        public float criticalDamageMod = 1;
        public float areaMod = 1;

        // New modifiers
        public int projectileCountBonus = 0;
        public int pierceCountBonus = 0;
        public float durationMod = 1;
        public int chainCountBonus = 0;
        public float cooldownReductionBonus = 0f;
    }
}