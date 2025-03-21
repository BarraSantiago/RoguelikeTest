using Entities;
using UnityEngine;
namespace Classes
{
    
    public class Arrow : MonoBehaviour
    {
        public float damage;
        public bool isPiercing;
    
        void OnTriggerEnter2D(Collider2D other)
        {
            // Handle collision with enemies
            if (other.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                other.GetComponent<Enemy>().TakeDamage(damage);
    
                // If the arrow is not piercing, return it to the pool
                if (!isPiercing)
                {
                    ReturnToPool();
                }
            }
        }
    
        void ReturnToPool()
        {
            // Return the arrow to the pool
            gameObject.SetActive(false);
            FindAnyObjectByType<ArrowPool>().ReturnArrow(gameObject);
        }
    }
}