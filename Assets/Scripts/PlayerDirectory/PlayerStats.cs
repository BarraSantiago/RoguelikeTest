using UnityEngine;

namespace PlayerDirectory
{
    public class PlayerStats : Stats
    {
        public PlayerStats(Classes classType)
        {
            switch (classType)
            {
                case Classes.Ranger:
                    health= 100f;
                    speed= 2f;
                    damage= 15f;
                    attackSpeed= 0.8f;
                    specialAttackSpeed= 3f;
                    projectileSpeed= 2f;
                    defenseSpeed= 1.5f;
                    range= 7f;
                    defense= 0.8f;
                    criticalChance= 0.07f;
                    criticalDamage= 2.15f;
                    area= 0f;
                    knockback= 0f;
                    knockbackResistance= 0f;
                    slowResistance= 0f;
                    break;
                case Classes.Warrior:
                    health = 150;
                    speed = 1.5f;
                    damage = 20;
                    attackSpeed = 0.8f;
                    specialAttackSpeed = 3f;
                    defenseSpeed = 1.5f;
                    range = 1.5f;
                    defense = 0.8f;
                    criticalChance = 0.09f;
                    criticalDamage = 2.15f;
                    area = 30f;
                    knockback = 15f;
                    break;
                //case Classes.Mage:
                //    damage = 8;
                //    attackSpeed = 1.2f;
                //    projectileSpeed = 8;
                //    range = 4;
                //    break;
                default:
                    damage = 10;
                    attackSpeed = 1;
                    projectileSpeed = 10;
                    range = 5;
                    break;
            }
        }
        public float health = 100;
        public float maxHealth = 100;
        public float speed = 5;
        public float specialAttackSpeed = 1;
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