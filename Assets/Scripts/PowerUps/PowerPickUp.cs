using UnityEngine;
using PlayerDirectory;

namespace PowerUps
{
    public class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private ScriptableObject powerUpObject;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobHeight = 0.5f;
        [SerializeField] private float bobSpeed = 1f;
        [SerializeField] private AudioClip pickupSound;
        
        private IPowerUp _powerUp;
        private Vector3 _startPosition;
        
        private void Start()
        {
            _powerUp = powerUpObject as IPowerUp;
            _startPosition = transform.position;
            
            if (_powerUp == null)
            {
                Debug.LogError("PowerUpPickup: The assigned powerUpObject does not implement IPowerUp!");
            }
        }
        
        private void Update()
        {
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
            
            float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            Player player = other.GetComponent<Player>();
            if (!player) return;
            
            _powerUp.OnPickup(player);
            
            if (pickupSound)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            } 
            
            Destroy(gameObject);
        }
    }
}