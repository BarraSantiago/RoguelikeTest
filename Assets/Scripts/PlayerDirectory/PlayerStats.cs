using UnityEngine;

namespace PlayerDirectory
{

    
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
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