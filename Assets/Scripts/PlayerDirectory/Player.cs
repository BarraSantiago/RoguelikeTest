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
    }
}