using System;
using System.Collections;
using Entities.XpDirectory;
using UnityEngine;

namespace Entities.EnemyDirectory
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public EnemyData data;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        public Action<Enemy> OnDeath { get; set; }
        public int frameImmunity = 10;
        public bool inmmune = false;
        private Rigidbody2D rigidbody;
        private bool canMove = true;
        public float experienceValue = 10f;
        public static EnemyManager enemyManager;
        public static ExperiencePool experiencePool;
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            rigidbody = GetComponent<Rigidbody2D>();
        }

        public void TakeDamage(float damage)
        {
            if(inmmune) return;
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
            spriteRenderer.color = Color.red;
            for (int i = 0; i < frameImmunity; i++)
            {
                yield return null; // Wait for one frame
            }

            inmmune = false;
            spriteRenderer.color = originalColor;
        }

        public void Move(Vector2 direction)
        {
            if(!canMove) return;
            rigidbody.MovePosition(rigidbody.position + direction * (data.speed * Time.deltaTime));
        }

        public void Knockback(Vector2 direction, float knockback)
        {
            StopAllCoroutines();
            rigidbody.AddForce(direction * knockback, ForceMode2D.Impulse);
            StartCoroutine(ContinueMoving());
        }

        public void Die()
        {
            OnDeath?.Invoke(this);
            enemyManager.HandleEnemyDeath(this);
            experiencePool.SpawnExperienceDrop(transform.position, experienceValue);
        }

        private IEnumerator ContinueMoving()
        {
            canMove = false;
            for (int i = 0; i < frameImmunity; i++)
            {
                yield return null;
            }
            rigidbody.linearVelocity = Vector2.zero;
            canMove = true;
        }

        public void ResetEnemy(EnemyData defaultData)
        {
            data = defaultData;
        }
    }
}