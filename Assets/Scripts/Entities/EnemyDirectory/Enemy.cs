using System;
using System.Collections;
using ClassesDirectory.Evolution;
using Entities.Pickables;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public EnemyData data;
        public Action<Enemy> OnDeath { get; set; }
        public float experienceValue = 10f;
        public static EnemyManager EnemyManager;
        public static ExperiencePool ExperiencePool;

        public static event Action<Enemy, float, bool> OnEnemyDamaged;
        
        private Color _originalColor;
        private EnemyType _enemyType = EnemyType.Normal;
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private bool _canMove = true;
        private Transform _transform;
        private bool _isSlowed;
        private float _slowMultiplier = 1f;
        private float _slowEndTime;
        private bool _isRooted;
        private float _rootEndTime;
        private float _originalSpeed;
        private Coroutine _knockbackCoroutine;
        
        public Vector2 Position => _transform.position;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            if (!_rigidbody) return;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            _originalColor = _spriteRenderer.color;
            _originalSpeed = data.speed;
            _canMove = true;
            _isSlowed = false;
            _isRooted = false;
        }

        private void Update()
        {
            UpdateStatusEffects();
        }

        private void UpdateStatusEffects()
        {
            if (_isSlowed && Time.time >= _slowEndTime)
            {
                RemoveSlow();
            }

            if (_isRooted && Time.time >= _rootEndTime)
            {
                RemoveRoot();
            }
        }

        public void Slow(float slowMultiplier, float duration)
        {
            _isSlowed = true;
            _slowMultiplier = slowMultiplier;
            _slowEndTime = Time.time + duration;

            if (!_isRooted)
            {
                _spriteRenderer.color = new Color(0.6f, 0.8f, 1f);
            }

            UpdateMovementSpeed();
        }

        public void Root(float duration)
        {
            _isRooted = true;
            _rootEndTime = Time.time + duration;
            _canMove = false;

            _spriteRenderer.color = new Color(0.8f, 0.4f, 1f);

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void RemoveSlow()
        {
            _isSlowed = false;
            _slowMultiplier = 1f;

            if (!_isRooted)
            {
                _spriteRenderer.color = _originalColor;
            }

            UpdateMovementSpeed();
        }

        private void RemoveRoot()
        {
            _isRooted = false;
            _canMove = true;

            if (_isSlowed)
            {
                _spriteRenderer.color = new Color(0.6f, 0.8f, 1f);
            }
            else
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private void UpdateMovementSpeed()
        {
            if (_isRooted)
            {
                data.speed = 0;
            }
            else
            {
                data.speed = _originalSpeed * _slowMultiplier;
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void TakeDamage(float damage, bool isCritical = false)
        {
            data.health -= damage;
            
            OnEnemyDamaged?.Invoke(this, damage, isCritical);
            
            StartCoroutine(DamageFlash());
        
            if (data.health <= 0)
            {
                Die();
            }
        }

        private IEnumerator DamageFlash()
        {
            Color flashColor = _isRooted ? new Color(1f, 0.4f, 1f) :
                _isSlowed ? new Color(0.8f, 1f, 1f) :
                Color.red;

            _spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);

            if (!gameObject.activeInHierarchy) yield break;

            if (_isRooted)
            {
                _spriteRenderer.color = new Color(0.8f, 0.4f, 1f);
            }
            else if (_isSlowed)
            {
                _spriteRenderer.color = new Color(0.6f, 0.8f, 1f);
            }
            else
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        public void Move(Vector2 direction)
        {
            if (!_canMove || !_rigidbody) return;
            
            // Use MovePosition for frame-rate independent movement
            Vector2 currentPosition = _rigidbody.position;
            Vector2 targetPosition = currentPosition + direction * (data.speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(targetPosition);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public void Knockback(Vector2 direction, float knockback)
        {
            if (_knockbackCoroutine != null)
            {
                StopCoroutine(_knockbackCoroutine);
            }
            
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * knockback, ForceMode2D.Impulse);
            _knockbackCoroutine = StartCoroutine(RecoverFromKnockback());
        }

        private IEnumerator RecoverFromKnockback()
        {
            _canMove = false;
            
            yield return new WaitForSeconds(0.2f);
            
            _rigidbody.linearVelocity = Vector2.zero;
            _canMove = true;
            _knockbackCoroutine = null;
        }

        public void Die()
        {
            OnDeath?.Invoke(this);
            EnemyManager.HandleEnemyDeath(this);
            ExperiencePool.SpawnExperienceDrop(transform.position, experienceValue);

            SubclassEvolutionManager evolutionManager = FindFirstObjectByType<SubclassEvolutionManager>();
            evolutionManager?.OnEnemyKilled();
        }

        public void ResetEnemy(EnemyData defaultData)
        {
            data = defaultData;
            _originalSpeed = data.speed;
            _originalColor = GetEnemyTypeColor();
            _spriteRenderer.color = _originalColor;
            _canMove = true;
            _isSlowed = false;
            _isRooted = false;
            _slowMultiplier = 1f;
            _rigidbody.linearVelocity = Vector2.zero;
            
            if (_knockbackCoroutine != null)
            {
                StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = null;
            }
        }

        public void SetEnemyType(EnemyType enemyType)
        {
            _enemyType = enemyType;
            _originalColor = GetEnemyTypeColor();
            _spriteRenderer.color = _originalColor;
        }

        private Color GetEnemyTypeColor()
        {
            return _enemyType switch
            {
                EnemyType.Elite => new Color(.7f, 0.2f, 0f),
                EnemyType.Boss => new Color(1f, 0f, 0f),
                _ => new Color(.5f, 0f, 0f)
            };
        }

        public bool IsDead() => data.health <= 0;
        public float GetHealth() => data.health;
        public bool IsRooted() => _isRooted;
        public bool IsSlowed() => _isSlowed;

        private void OnDisable()
        {
            _isSlowed = false;
            _isRooted = false;
            _slowMultiplier = 1f;
            _canMove = true;
            data.speed = _originalSpeed;
            _rigidbody.linearVelocity = Vector2.zero;
            
            if (_knockbackCoroutine != null)
            {
                StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = null;
            }
        }
    }
}