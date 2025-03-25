using System;
using Classes;
using UnityEngine;

namespace PlayerDirectory
{
    public class Player : MonoBehaviour
    {
        public PlayerStats playerStats;
        public IClass playerClass;
        private float lastAttackTime;
        private float lastSpecialAttackTime;
        private float lastDefenseTime;


        private void Awake()
        {
            Ranger ranger = new Ranger
            {
                arrowPool = gameObject.AddComponent<ArrowPool>()
            };
            playerClass = ranger;
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
            if (CanAttack())
            {
                playerClass.BasicAttack(playerStats);
                lastAttackTime = Time.time;
            }
        }

        public void SpecialAttack()
        {
            if (CanSpecialAttack())
            {
                playerClass.SpecialAttack(playerStats);
                lastSpecialAttackTime = Time.time;
            }
        }

        public void Defense()
        {
            if (CanDefense())
            {
                playerClass.Defense(playerStats);
                lastDefenseTime = Time.time;
            }
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