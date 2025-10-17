using System.Collections.Generic;
using Entities.EnemyDirectory;
using UnityEngine;

namespace Classes.RangerClass
{
    public class Arrow : MonoBehaviour
    {
        public float damage;
        public bool isPiercing;
        private Vector2 _startPosition;
        public float range;
        private HashSet<Enemy> _hitEnemies = new();


        private void OnEnable()
        {
            _startPosition = transform.position;
        }
        
        private void Update()
        {
            if (Vector2.Distance(_startPosition, transform.position) >= range)
            {
                ReturnToPool();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (_hitEnemies.Contains(enemy)) return;

            _hitEnemies.Add(enemy);
            enemy.TakeDamage(damage);

            if (isPiercing)
            {
                damage *= 0.9f; // Reduce damage by 20% after each hit
            }
            else
            {
                ReturnToPool();
            }
        }


        private void ReturnToPool()
        {
            _hitEnemies.Clear();
            gameObject.SetActive(false);
            FindAnyObjectByType<ArrowPool>().ReturnArrow(gameObject);
        }
    }
}