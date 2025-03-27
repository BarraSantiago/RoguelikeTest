using System;
using UnityEngine;

namespace PlayerDirectory
{
    [CreateAssetMenu(fileName = "StatsModifier", menuName = "Scriptable Objects/StatsModifiers")][Serializable]
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

}