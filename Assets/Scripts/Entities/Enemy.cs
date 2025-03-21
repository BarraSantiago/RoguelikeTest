using System;
using UnityEngine;

namespace Entities
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public EnemyData data;

        public Action<Enemy> OnDeath { get; set; }

        public void TakeDamage(float damage)
        {
            data.health -= damage;
            if (data.health <= 0)
            {
                OnDeath?.Invoke(this);
                FindObjectOfType<EnemyManager>().HandleEnemyDeath(this);
            }
        }

        public void Move(Vector2 direction)
        {
            // Just executes movement - logic comes from manager
            transform.Translate(direction * (data.speed * Time.deltaTime));
        }

        // Reset state when reactivated from pool
        public void ResetEnemy(EnemyData defaultData)
        {
            data = defaultData;
        }
    }
}