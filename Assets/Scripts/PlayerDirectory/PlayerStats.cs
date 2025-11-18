using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
    public class PlayerStats : Stats
    {
        public float health;
        public float maxHealth;
        public float speed;
        public float specialAttackSpeed;
        public float defenseSpeed;
        public float defense;
        public float knockback;
        public float knockbackResistance;
        public float slowResistance;

        // New stats
        public float healthRegen = 0f; // HP per second
        public float armor = 0f; // Flat damage reduction
        public int revival = 0; // Number of revivals available
        public float pickupRadius = 2f; // XP gem collection radius
        public float invincibilityDuration = 1f; // I-frames after hit
        public float magnetism = 1f; // Pickup attraction strength
        public float luck = 1f; // Affects drops and procs
        public int maxWeapons = 6; // Weapon slot limit
        public int maxPassives = 6; // Passive item limit
        public float greed = 1f; // Gold/coin multiplier
        public float growth = 1f; // XP gain multiplier
        public float curse = 0f; // Enemy HP/count multiplier
        public int reroll = 0; // Level-up reroll charges
        public int skip = 0; // Level-up skip charges
        public int banish = 0; // Level-up banish charges
        public int projectileCount = 1; // Base projectile count
        public int pierceCount = 0; // Projectiles pierce through enemies
        public float cooldownReduction = 0f; // Already exists
        
    }
}