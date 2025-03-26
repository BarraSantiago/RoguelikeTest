using UnityEngine;
using PlayerDirectory;

namespace Entities.XpDirectory
{
    public class ExperienceDrop : MonoBehaviour
    {
        public float experienceAmount = 1f;
        public float attractRadius = 5f;
        public float moveSpeed = 5f;
        public float accelerationRate = 0.5f;
        
        private bool isAttracting = false;
        private float currentSpeed;
        public static Player player;
        private ExperiencePool pool;
        
     
        
        public void Initialize(Vector3 position, float expAmount, ExperiencePool expPool)
        {
            transform.position = position;
            experienceAmount = expAmount;
            pool = expPool;
            isAttracting = false;
            currentSpeed = moveSpeed;
            gameObject.SetActive(true);
        }
        
        private void Update()
        {
            if (!player) return;
            
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            // Start attracting when within radius
            if (distance <= attractRadius)
            {
                isAttracting = true;
            }

            if (!isAttracting) return;
            
            Vector3 direction = (player.transform.position - transform.position).normalized;
            currentSpeed += accelerationRate * Time.deltaTime;
            transform.position += direction * (currentSpeed * Time.deltaTime);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            Player playerComponent = other.GetComponent<Player>();
            
            if (playerComponent)
            {
                playerComponent.GainExperience(experienceAmount);
            }
            
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            gameObject.SetActive(false);
            pool.ReturnExperienceDrop(this);
        }
    }
}