using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        public float health;
        public float speed;
        public float damage;
        public float attackSpeed;
        public float range;
        public float jumpHeight;
        public float defense;
        public float criticalChance;
        public float criticalDamage;
        public float knockback;
        public float knockbackResistance;
        public float slowResistance;
    }
}