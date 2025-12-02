using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace Entities.Pickables
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

        private Rigidbody2D _rb;
        private Vector3 _targetPosition;

        private void Awake()
        {
            // Ensure a Rigidbody2D exists so triggers/collisions work when player has no Rigidbody2D
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
            _rb.simulated = true;
            // optional: _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Add to ExperienceDrop.cs Initialize method
        public void Initialize(Vector3 position, float expAmount, ExperiencePool expPool)
        {
            transform.position = position;
            
            // Apply greed multiplier
            if (Player)
            {
                expAmount = CombatUtilities.ApplyGreed(expAmount, 
                    Player.PlayerBaseStats.Greed, Player.baseStatsMod.greedMod);
            }
            
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

            if (_isAttracting)
            {
                _targetPosition = Player.transform.position;
                _currentSpeed += accelerationRate * Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (!_isAttracting || !_rb) return;

            Vector3 direction = (_targetPosition - transform.position).normalized;
            Vector3 next = transform.position + direction * (_currentSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(next);
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