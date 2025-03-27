using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "StatsModifier", menuName = "Scriptable Objects/Stats")]
    public class StatsModifier : ScriptableObject
    {
        public float damageMod = 1;
        public float attackSpeedMod = 1;
        public float projectileSpeedMod = 1;
        public float rangeMod = 1;
        public float criticalChanceMod = 1;
        public float criticalDamageMod = 1;
        public float areaMod = 1;
        
    }

    [CreateAssetMenu(fileName = "PlayerStatsModifier", menuName = "Scriptable Objects/Stats")]
    public class PlayerStatsModifier : StatsModifier
    {
        public float speedMod = 1;
        public float sAttackSpeedMod = 1;
        public float defenseSpeedMod = 1;
        public float defenseMod = 1;
        public float knockbackMod = 1;
        public float slowResistanceMod = 1;
    }
}