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
    }
    
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/Stats")]
    public class PlayerStats : Stats
    {
        public float health;
        public float speed;
        public float specialAttackSpeed;
        public float defenseSpeed;
        public float defense;
        public float knockback;
        public float knockbackResistance;
        public float slowResistance;
    }
}