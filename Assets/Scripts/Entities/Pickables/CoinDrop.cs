using Achievements;
using PlayerDirectory;
using UnityEngine;

namespace Entities.Pickables
{
    public class CoinDrop : MonoBehaviour
    {
        [SerializeField] private float attractRadius = 5f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float accelerationRate = 0.5f;

        private CoinPool _pool;
        private float _coinValue = 1f;
        private bool _isAttracting = false;
        private float _currentSpeed;
        private Player _player;
        private Rigidbody2D _rb;
        private Vector3 _targetPosition;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
            _rb.simulated = true;

            _player = FindFirstObjectByType<Player>();
        }

        public void Initialize(float value)
        {
            _coinValue = value;
            _isAttracting = false;
            _currentSpeed = moveSpeed;
        }

        private void Update()
        {
            if (!_player) return;

            float distance = Vector3.Distance(transform.position, _player.transform.position);

            if (distance <= attractRadius)
            {
                _isAttracting = true;
            }

            if (_isAttracting)
            {
                _targetPosition = _player.transform.position;
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

        public void Initialize(float value, CoinPool pool = null)
        {
            _coinValue = value;
            _isAttracting = false;
            _currentSpeed = moveSpeed;
            _pool = pool;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Player playerComponent = other.GetComponent<Player>();

            if (playerComponent)
            {
                int coinAmount = Mathf.RoundToInt(_coinValue);
                playerComponent.OnCoinCollected(coinAmount);
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_pool != null)
            {
                gameObject.SetActive(false);
                _pool.Return(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}