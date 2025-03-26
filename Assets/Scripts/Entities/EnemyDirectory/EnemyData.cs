namespace Entities.EnemyDirectory
{
    [System.Serializable]
    public struct EnemyData
    {
        public float health;
        public float speed;
        public float attackCooldown;
        public float attackDamage;
        public float attackRange;
        public float lastAttackTime;
    }
}