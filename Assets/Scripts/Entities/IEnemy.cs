using UnityEngine;

namespace Entities
{
    public interface IEnemy
    {
        void TakeDamage(float damage);
        void Move(Vector2 direction);
    }
}