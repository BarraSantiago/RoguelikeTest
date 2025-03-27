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

    public enum Classes
    {
        Warrior,
        Ranger
    }

    public class Player : MonoBehaviour
    {
        [SerializeField] private Classes playerClassType;
        public PlayerStats playerStats;
        public PlayerStatsModifier statsMod;
        private IClass playerClass;
        private float lastAttackTime;
        private float lastSpecialAttackTime;
        private float lastDefenseTime;
        private Direction direction;
        private float currentHp = 100;
        public float currentExperience = 0f;
        public float experienceToNextLevel = 100f;
        public int level = 1;

        private void Awake()
        {
            Ranger ranger = new Ranger
            {
                arrowPool = gameObject.AddComponent<ArrowPool>()
            };
            switch (playerClassType)
            {
                case Classes.Warrior:
                    playerClass = new Warrior();
                    break;
                case Classes.Ranger:
                    playerClass = ranger;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            playerStats = Resources.Load<PlayerStats>("scriptableObjects/" + playerClassType.ToString() + "Stats");

            currentHp = playerStats.health;
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
            playerClass.BasicAttack(playerStats, statsMod, direction);
            lastAttackTime = Time.time;
        }

        public void SpecialAttack()
        {
            if (!CanSpecialAttack()) return;
            playerClass.SpecialAttack(playerStats, statsMod, direction);
            lastSpecialAttackTime = Time.time;
        }

        public void Defense()
        {
            if (!CanDefense()) return;
            playerClass.Defense(playerStats, statsMod, direction);
            lastDefenseTime = Time.time;
        }

        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + playerStats.attackSpeed * statsMod.attackSpeedMod;
        }

        private bool CanSpecialAttack()
        {
            return Time.time >= lastSpecialAttackTime + playerStats.specialAttackSpeed * statsMod.sAttackSpeedMod;
        }

        private bool CanDefense()
        {
            return Time.time >= lastDefenseTime + playerStats.defenseSpeed * statsMod.defenseSpeedMod;
        }

        public void TakeDamage(float attackDamage)
        {
            currentHp -= attackDamage * playerStats.defense;

            if (currentHp <= 0)
            {
                // TODO Death
            }
        }

        public void GainExperience(float amount)
        {
            currentExperience += amount;

            if (currentExperience < experienceToNextLevel) return;
            level++;
            currentExperience -= experienceToNextLevel;
            experienceToNextLevel *= 1.2f;

            // TODO Make lvl up
        }

        public void Move(Vector2 direction)
        {
            transform.Translate(direction * (playerStats.speed * Time.deltaTime));
        }
    }
}