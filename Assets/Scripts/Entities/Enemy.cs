using PlayerDirectory;
using UnityEngine;

namespace Entities
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public float health;
        public float speed;
        public float attackCooldown;
        public float attackDamage;
        public float attackRange;
        private float lastAttackTime;
        public Player target;
        private void Update()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player && Vector2.Distance(transform.position, player.transform.position) <= attackRange)
            {
                if (CanAttack())
                {
                    Attack(target);
                    lastAttackTime = Time.time;
                }
            }
        }
        
        public void Attack(Player player)
        {
            player.TakeDamage(attackDamage);
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }

        public void Move(Vector2 direction)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        private void Die()
        {
            gameObject.SetActive(false);
            FindAnyObjectByType<EnemyPool>().ReturnEnemy(gameObject);
        }
        
        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }
    }
}