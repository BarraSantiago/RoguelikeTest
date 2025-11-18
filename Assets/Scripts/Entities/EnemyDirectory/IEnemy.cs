using UnityEngine;

namespace Entities.EnemyDirectory
{
    public enum EnemyType
    {
        Normal,
        Elite,
        Boss
    }
    public interface IEnemy
    {
        public void SetPosition(Vector3 position);
        public void TakeDamage(float damage, bool isCritical);
        public void Move(Vector2 direction);
        public void Knockback(Vector2 direction, float knockback);
        public void Die();
    }
}