using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "PlayerStatsModifier", menuName = "Scriptable Objects/PlayerStatsModifier")]
    public class PlayerStatsModifier : StatsModifier
    {
        public float speedMod = 1;
        public float sAttackSpeedMod = 1;
        public float defenseSpeedMod = 1;
        public float defenseMod = 1;
        public float knockbackMod = 1;
        public float slowResistanceMod = 1;

        // New player-specific modifiers
        public float healthRegenBonus = 0f;
        public float armorBonus = 0f;
        public float pickupRadiusMod = 1;
        public float magnetismMod = 1;
        public float luckMod = 1;
        public float greedMod = 1;
        public float growthMod = 1;
        public float curseBonus = 0f;
        public int revivalBonus = 0;
        public int rerollBonus = 0;
        public int skipBonus = 0;
        public int banishBonus = 0;
        public int projectileCountBonus = 0;
        public int pierceCountBonus = 0;
        public float durationMod = 1f; // Affects projectile lifetime, DoT, buffs
        
    }
}