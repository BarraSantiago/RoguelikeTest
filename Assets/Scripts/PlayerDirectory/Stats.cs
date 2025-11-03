using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "Stats", menuName = "Scriptable Objects/Stats")]
    public class Stats : ScriptableObject
    {
        public float damage;
        public float attackSpeed;
        public float projectileSpeed;
        public float range;
        public float criticalChance = 0.10f;
        public float criticalDamage = 2.0f;
        public float area;

        // New stats
        public int projectileCount = 1;
        public int pierceCount = 0; // How many enemies projectile can pierce
        public float duration = 5f; // Projectile/effect lifetime
        public int chainCount = 0; // Chain lightning effects
        public float cooldownReduction = 0f; // Percentage 0-1
    }
}