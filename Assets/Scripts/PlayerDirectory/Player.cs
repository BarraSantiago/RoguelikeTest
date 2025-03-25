using System;
using Classes;
using Classes.RangerClass;
using Classes.WarriorClass;
using UnityEngine;

namespace PlayerDirectory
{
    public struct Direction
    {
        public Vector2 position;
        public Vector2 direction;
    }
    public class Player : MonoBehaviour
    {
        public PlayerStats playerStats;
        public IClass playerClass;
        private float lastAttackTime;
        private float lastSpecialAttackTime;
        private float lastDefenseTime;
        private Direction direction;
        
        private void Awake()
        {
            Ranger ranger = new Ranger
            {
                arrowPool = gameObject.AddComponent<ArrowPool>()
            };
            playerClass = new Warrior();
        }

        private void Update()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 newDirection = (mousePosition - transform.position).normalized;
            direction = new Direction()
            {
                position = transform.position,
                direction = newDirection
            };
            
        }

        public Player(PlayerStats playerStats, IClass playerClass)
        {
            this.playerStats = playerStats;
            this.playerClass = playerClass;
            lastAttackTime = -playerStats.attackSpeed;
            lastSpecialAttackTime = -playerStats.specialAttackSpeed;
            lastDefenseTime = -playerStats.defenseSpeed;
        }

        public void BasicAttack()
        {
            if (!CanAttack()) return;
            playerClass.BasicAttack(playerStats, direction);
            lastAttackTime = Time.time;
        }

        public void SpecialAttack()
        {
            if (!CanSpecialAttack()) return;
            playerClass.SpecialAttack(playerStats, direction);
            lastSpecialAttackTime = Time.time;
        }

        public void Defense()
        {
            if (!CanDefense()) return;
            playerClass.Defense(playerStats, direction);
            lastDefenseTime = Time.time;
        }

        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + playerStats.attackSpeed;
        }

        private bool CanSpecialAttack()
        {
            return Time.time >= lastSpecialAttackTime + playerStats.specialAttackSpeed;
        }

        private bool CanDefense()
        {
            return Time.time >= lastDefenseTime + playerStats.defenseSpeed;
        }

        public void TakeDamage(float attackDamage)
        {
            playerStats.health -= attackDamage * playerStats.defense;
        }
        
        public void Move(Vector2 direction)
        {
            transform.Translate(direction * (playerStats.speed * Time.deltaTime));
        }
    }
}