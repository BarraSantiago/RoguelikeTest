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

        private static readonly Dictionary<Collider2D, Enemy> ColliderToEnemy = new();

        private Collider2D _collider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
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
            for (int i = 0; i < frameImmunity; i++)
            {
                yield return null; // Wait for one frame
            }

            inmmune = false;
            _spriteRenderer.color = _originalColor;
        }

        public void Move(Vector2 direction)
        {
            if (!_canMove) return;
            _rigidbody.MovePosition(_rigidbody.position + direction * (data.speed * Time.deltaTime));
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

        private void OnDestroy()
        {
            if (_collider)
            {
                ColliderToEnemy.Remove(_collider);
            }
        }
    }
}