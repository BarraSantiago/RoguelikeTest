using System;
using System.Collections;
using System.Collections.Generic;
using Classes.Evolution;
using Entities.XpDirectory;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public EnemyData data;
        public Action<Enemy> OnDeath { get; set; }
        public int frameImmunity = 10;
        public bool inmmune = false;
        public float experienceValue = 10f;
        public static EnemyManager EnemyManager;
        public static ExperiencePool ExperiencePool;

        private Color _originalColor;
        private EnemyType _enemyType = EnemyType.Normal;
        private static readonly Dictionary<Collider2D, Enemy> ColliderToEnemy = new();
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private bool _canMove = true;
        private Transform _transform;
        private WaitForSeconds _frameWait;
        private bool _isSlowed;
        private float _slowMultiplier = 1f;
        private float _slowEndTime;
        private bool _isRooted;
        private float _rootEndTime;
        private float _originalSpeed;
        public Vector2 Position => _transform.position;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _frameWait = new WaitForSeconds(Time.fixedDeltaTime);

            if (_collider != null)
            {
                ColliderToEnemy[_collider] = this;
            }

            _originalSpeed = data.speed;
        }

        private void Update()
        {
            UpdateStatusEffects();
        }

        private void UpdateStatusEffects()
        {
            // Update slow
            if (_isSlowed && Time.time >= _slowEndTime)
            {
                RemoveSlow();
            }

            // Update root
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

            // Visual feedback - blue tint
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

            // Visual feedback - purple tint
            _spriteRenderer.color = new Color(0.8f, 0.4f, 1f);

            // Stop movement
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

        public void TakeDamage(float damage)
        {
            if (inmmune) return;

            data.health -= damage;
            StartCoroutine(DamageFlash());
            StartCoroutine(FrameInvulnerability());

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

        private IEnumerator FrameInvulnerability()
        {
            inmmune = true;
            yield return _frameWait;
            inmmune = false;
        }

        public void Move(Vector2 direction)
        {
            if (!_canMove) return;
            Vector2 movement = direction * (data.speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }

        public void Knockback(Vector2 direction, float knockback)
        {
            StopAllCoroutines();
            _rigidbody.AddForce(direction * knockback, ForceMode2D.Impulse);
            StartCoroutine(ContinueMoving());
        }

        public void Die()
        {
            OnDeath?.Invoke(this);
            EnemyManager.HandleEnemyDeath(this);
            ExperiencePool.SpawnExperienceDrop(transform.position, experienceValue);

            SubclassEvolutionManager evolutionManager =
                FindFirstObjectByType<Classes.Evolution.SubclassEvolutionManager>();
            evolutionManager?.OnEnemyKilled();
        }

        private IEnumerator ContinueMoving()
        {
            _canMove = false;
            for (int i = 0; i < frameImmunity; i++)
            {
                yield return null;
            }

            _rigidbody.linearVelocity = Vector2.zero;
            _canMove = true;
        }

        public void ResetEnemy(EnemyData defaultData)
        {
            data = defaultData;
        }

        public static Enemy GetFromCollider(Collider2D collider)
        {
            return ColliderToEnemy.TryGetValue(collider, out Enemy enemy) ? enemy : null;
        }

        public void SetEnemyType(EnemyType enemyType)
        {
            _enemyType = enemyType;
            ApplyVisualEffects();
        }

        private void ApplyVisualEffects()
        {
            switch (_enemyType)
            {
                case EnemyType.Normal:
                    _spriteRenderer.color = new Color(.5f, 0f, 0f); // Normal color
                    break;
                case EnemyType.Elite:
                    _spriteRenderer.color = new Color(.7f, 0.2f, 0f); // Golden tint
                    break;
                case EnemyType.Boss:
                    _spriteRenderer.color = new Color(1f, 0f, 0f); // Red tint
                    break;
            }
        }


        public bool IsDead() => data.health <= 0;
        public float GetHealth() => data.health;
        public bool IsRooted() => _isRooted;
        public bool IsSlowed() => _isSlowed;

        private void OnDisable()
        {
            // Reset status effects when returned to pool
            _isSlowed = false;
            _isRooted = false;
            _slowMultiplier = 1f;
            _canMove = true;
            data.speed = _originalSpeed;
            _spriteRenderer.color = _originalColor;
        }
        
        private void OnDestroy()
        {
            if (_collider)
            {
                ColliderToEnemy.Remove(_collider);
            }
        }
    }
}