using PlayerDirectory;
using UnityEngine;

namespace Entities
{
    public interface IEnemy
    {
        void Attack(Player player);
        void TakeDamage(float damage);
        void Move(Vector2 direction);
    }
}