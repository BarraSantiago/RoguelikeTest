using System;
using System.Collections;
using System.Collections.Generic;
using Entities.XpDirectory;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public EnemyData data;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        public Action<Enemy> OnDeath { get; set; }
        public int frameImmunity = 10;
        public bool inmmune = false;
        private Rigidbody2D _rigidbody;
        private bool _canMove = true;
        public float experienceValue = 10f;
        public static EnemyManager EnemyManager;
        public static ExperiencePool ExperiencePool;

        private EnemyType _enemyType = EnemyType.Normal;
        private static readonly Dictionary<Collider2D, Enemy> ColliderToEnemy = new();
        private Collider2D _collider;

        private Transform _transform;
        private WaitForSeconds _frameWait;
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
        }



        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void TakeDamage(float damage)
        {
            if (inmmune) return;
            data.health -= damage;
            StartCoroutine(FlashRed());

            if (data.health <= 0)
            {
                Die();
            }
        }

        private IEnumerator FlashRed()
        {
            inmmune = true;
            _spriteRenderer.color = Color.red;

            yield return _frameWait;

            inmmune = false;
            _spriteRenderer.color = _originalColor;
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
                case EnemyType.Elite:
                    _spriteRenderer.color = new Color(1f, 0.8f, 0f); // Golden tint
                    break;
                case EnemyType.Boss:
                    _spriteRenderer.color = new Color(0.8f, 0f, 0f); // Red tint
                    break;
            }
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