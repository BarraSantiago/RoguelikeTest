using UnityEngine;

namespace Entities
{
    public interface IEnemy
    {
        public void TakeDamage(float damage);
        public void Move(Vector2 direction);
        public void Knockback(Vector2 direction, float knockback);
    }
}