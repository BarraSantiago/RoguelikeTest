using System;
using System.Collections.Generic;
using Classes;
using Classes.RangerClass;
using Classes.WarriorClass;
using Entities.EnemyDirectory;
using PlayerDirectory.LevelingSystem;
using PowerUps;
using UnityEngine;
using Utilities;
using View.UI.Leveling;

namespace PlayerDirectory
{
    public struct Vec
    {
        public Vector2 Position;
        public Vector2 Direction;
    }

    public enum Classes
    {
        Warrior,
        Ranger
    }

    public class Player : MonoBehaviour
    {
        [SerializeField] private Classes playerClassType;
        [SerializeField] private bool _autoTargetEnabled;
        [SerializeField] private bool _autoAttackEnabled;
        [SerializeField] private float _autoAttackRange = 10f;

        public PlayerStats playerStats;
        public PlayerStatsModifier statsMod;
        public List<PowerUp> powerUps;
        public LevelSystem levelSystem = new();
        
        private LevelUpManager _levelUpManager;
        private IClass _playerClass;
        private float _lastAttackTime;
        private float _lastSpecialAttackTime;
        private float _lastDefenseTime;
        private Vec _vec;
        private float _currentHp = 100;
        private LevelUpUI _levelUpUI;
        private Camera _mainCamera;
        private void Awake()
        {
            switch (playerClassType)
            {
                case Classes.Warrior:
                    _playerClass = new Warrior();
                    break;
                case Classes.Ranger:
                    Ranger ranger = new Ranger
                    {
                        ArrowPool = gameObject.AddComponent<ArrowPool>()
                    };
                    _playerClass = ranger;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            playerStats = Resources.Load<PlayerStats>("scriptableObjects/ClassesStats/" + playerClassType.ToString() + "Stats");

            _currentHp = playerStats.health;
            _levelUpManager = FindFirstObjectByType<LevelUpManager>();
            _levelUpUI = FindFirstObjectByType<LevelUpUI>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 newDirection = (mousePosition - transform.position).normalized;
            
            _vec = new Vec()
            {
                Position = transform.position,
                Direction = newDirection
            };

            foreach (PowerUp powerUp in powerUps)
            {
                powerUp.Activate(this);
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                GainExperience(100f);
            }

            if (CanSpecialAttack() || CanAttack())
            {
                HandleAutoTargeting();
            }
        }

        public Player(PlayerStats playerStats, IClass playerClass)
        {
            this.playerStats = playerStats;
            this._playerClass = playerClass;
            _lastAttackTime = -playerStats.attackSpeed;
            _lastSpecialAttackTime = -playerStats.specialAttackSpeed;
            _lastDefenseTime = -playerStats.defenseSpeed;
        }

        public void BasicAttack()
        {
            if (!CanAttack()) return;
            _playerClass.BasicAttack(playerStats, statsMod, _vec);
            _lastAttackTime = Time.time;
        }

        public void SpecialAttack()
        {
            if (!CanSpecialAttack()) return;
            _playerClass.SpecialAttack(playerStats, statsMod, _vec);
            _lastSpecialAttackTime = Time.time;
        }

        public void Defense()
        {
            if (!CanDefense()) return;
            _playerClass.Defense(playerStats, statsMod, _vec);
            _lastDefenseTime = Time.time;
        }

        private bool CanAttack()
        {
            return Time.time >= _lastAttackTime + playerStats.attackSpeed * statsMod.attackSpeedMod;
        }

        private bool CanSpecialAttack()
        {
            return Time.time >= _lastSpecialAttackTime + playerStats.specialAttackSpeed * statsMod.sAttackSpeedMod;
        }

        private bool CanDefense()
        {
            return Time.time >= _lastDefenseTime + playerStats.defenseSpeed * statsMod.defenseSpeedMod;
        }

        public void TakeDamage(float attackDamage)
        {
            _currentHp -= attackDamage * playerStats.defense;

            if (_currentHp <= 0)
            {
                // TODO Death
            }
        }

        public void GainExperience(float amount)
        {
            bool leveledUp = levelSystem.AddExperience(amount);

            if (leveledUp)
            {
                OnLevelUp();
            }
        }

        private void OnLevelUp()
        {
            List<object> choices = _levelUpManager.GenerateLevelUpChoices(this);
            _levelUpUI.ShowLevelUpChoices(this, choices);
        }
        
        public void Move(Vector2 direction)
        {
            transform.Translate(direction * (playerStats.speed * Time.deltaTime));
        }
        
        public float SpecialAttackCooldownProgress => 
            Mathf.Clamp01((Time.time - _lastSpecialAttackTime) / (playerStats.specialAttackSpeed * statsMod.sAttackSpeedMod));

        public float DefenseCooldownProgress => 
            Mathf.Clamp01((Time.time - _lastDefenseTime) / (playerStats.defenseSpeed * statsMod.defenseSpeedMod));

        public bool AutoTargetEnabled 
        { 
            get => _autoTargetEnabled; 
            set => _autoTargetEnabled = value; 
        }

        public bool AutoAttackEnabled 
        { 
            get => _autoAttackEnabled; 
            set => _autoAttackEnabled = value; 
        }

        private void HandleAutoTargeting()
        {
            if (!_autoTargetEnabled || !_autoAttackEnabled) return;

            Enemy nearestEnemy = CombatUtilities.GetNearestEnemy(transform.position, _autoAttackRange);
            if (!nearestEnemy) return;

            Vector2 directionToEnemy = ((Vector2)nearestEnemy.transform.position - (Vector2)transform.position).normalized;
            _vec.Direction = directionToEnemy;
            
            BasicAttack();
            
            SpecialAttack();
            
        }
    }
}