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
}