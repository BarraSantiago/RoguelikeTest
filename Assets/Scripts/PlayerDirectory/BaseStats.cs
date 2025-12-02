namespace PlayerDirectory
{
    public class BaseStats 
    {
        public float Damage;
        public float AttackSpeed;
        public float ProjectileSpeed;
        public float Range;
        public float CriticalChance = 0.10f;
        public float CriticalDamage = 2.0f;
        public float Area;

        // New stats
        public int ProjectileCount = 1;
        public int PierceCount = 0; // How many enemies projectile can pierce
        public float Duration = 5f; // Projectile/effect lifetime
        public int ChainCount = 0; // Chain lightning effects
        public float CooldownReduction = 0f; // Percentage 0-1
    }
}