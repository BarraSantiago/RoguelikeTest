using ClassesDirectory;

namespace PlayerDirectory
{
    public class PlayerBaseStats : BaseStats
    {
        public PlayerBaseStats(Classes classType)
        {
            switch (classType)
            {
                case Classes.Ranger:
                    Health= 100f;
                    Speed= 2f;
                    Damage= 15f;
                    AttackSpeed= 0.8f;
                    SpecialAttackSpeed= 3f;
                    ProjectileSpeed= 2f;
                    DefenseSpeed= 1.5f;
                    Range= 7f;
                    Defense= 0.8f;
                    CriticalChance= 0.07f;
                    CriticalDamage= 2.15f;
                    Area= 0f;
                    Knockback= 0f;
                    KnockbackResistance= 0f;
                    SlowResistance= 0f;
                    break;
                case Classes.Warrior:
                    Health = 150;
                    Speed = 1.5f;
                    Damage = 20;
                    AttackSpeed = 0.8f;
                    SpecialAttackSpeed = 3f;
                    DefenseSpeed = 1.5f;
                    Range = 1.5f;
                    Defense = 0.8f;
                    CriticalChance = 0.09f;
                    CriticalDamage = 2.15f;
                    Area = 30f;
                    Knockback = 15f;
                    break;
                //case Classes.Mage:
                //    damage = 8;
                //    attackSpeed = 1.2f;
                //    projectileSpeed = 8;
                //    range = 4;
                //    break;
                default:
                    Damage = 10;
                    AttackSpeed = 1;
                    ProjectileSpeed = 10;
                    Range = 5;
                    break;
            }
        }
        public float Health = 100;
        public float MaxHealth = 100;
        public float Speed = 5;
        public float SpecialAttackSpeed = 1;
        public float DefenseSpeed;
        public float Defense;
        public float Knockback;
        public float KnockbackResistance;
        public float SlowResistance;

        // New stats
        public float HealthRegen = 0f; // HP per second
        public float Armor = 0f; // Flat damage reduction
        public int Revival = 0; // Number of revivals available
        public float PickupRadius = 2f; // XP gem collection radius
        public float InvincibilityDuration = 1f; // I-frames after hit
        public float Magnetism = 1f; // Pickup attraction strength
        public float Luck = 1f; // Affects drops and procs
        public int MaxWeapons = 6; // Weapon slot limit
        public int MaxPassives = 6; // Passive item limit
        public float Greed = 1f; // Gold/coin multiplier
        public float Growth = 1f; // XP gain multiplier
        public float Curse = 0f; // Enemy HP/count multiplier
        public int Reroll = 0; // Level-up reroll charges
        public int Skip = 0; // Level-up skip charges
        public int Banish = 0; // Level-up banish charges
        
    }
}