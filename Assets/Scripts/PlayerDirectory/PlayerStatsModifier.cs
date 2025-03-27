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
    }
}