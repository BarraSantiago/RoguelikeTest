using System.Collections;
using Entities;
using UnityEngine;

namespace Classes.RangerClass
{
    public class Arrow : MonoBehaviour
    {
        public float damage;
        public bool isPiercing;
        public float piercingDuration = 2f;


        private void OnEnable()
        {
            if (isPiercing)
            {
                StartCoroutine(DeactivateAfterTime(piercingDuration));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Arrow hit something");
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

        private IEnumerator DeactivateAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            // Return the arrow to the pool
            gameObject.SetActive(false);
            FindAnyObjectByType<ArrowPool>().ReturnArrow(gameObject);
        }
    }
}