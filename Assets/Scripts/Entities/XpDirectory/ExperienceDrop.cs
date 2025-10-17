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
        
        private bool _isAttracting = false;
        private float _currentSpeed;
        public static Player Player;
        private ExperiencePool _pool;
        
     
        
        public void Initialize(Vector3 position, float expAmount, ExperiencePool expPool)
        {
            transform.position = position;
            experienceAmount = expAmount;
            _pool = expPool;
            _isAttracting = false;
            _currentSpeed = moveSpeed;
            gameObject.SetActive(true);
        }
        
        private void Update()
        {
            if (!Player) return;
            
            float distance = Vector3.Distance(transform.position, Player.transform.position);
            
            // Start attracting when within radius
            if (distance <= attractRadius)
            {
                _isAttracting = true;
            }

            if (!_isAttracting) return;
            
            Vector3 direction = (Player.transform.position - transform.position).normalized;
            _currentSpeed += accelerationRate * Time.deltaTime;
            transform.position += direction * (_currentSpeed * Time.deltaTime);
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
            _pool.Return(this);
        }
    }
}