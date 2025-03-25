using System.Collections;
using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Classes.RangerClass
{
    public class Arrow : MonoBehaviour
    {
        public float damage;
        public bool isPiercing;
        private Vector2 startPosition;
        public float range;
        private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();


        private void OnEnable()
        {
            startPosition = transform.position;
        }
        
        private void Update()
        {
            if (Vector2.Distance(startPosition, transform.position) >= range)
            {
                ReturnToPool();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (hitEnemies.Contains(enemy)) return;

            hitEnemies.Add(enemy);
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
            hitEnemies.Clear();
            gameObject.SetActive(false);
            FindAnyObjectByType<ArrowPool>().ReturnArrow(gameObject);
        }
    }
}