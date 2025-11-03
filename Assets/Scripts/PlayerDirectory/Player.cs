using System;
using System.Collections;
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
        [SerializeField] private bool _autoTargetEnabled = true;
        [SerializeField] private bool _autoAttackEnabled = true;
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
        private Vector2 _manualTargetDirection;
        private static readonly int MaxPowerUps = 10; // Reasonable limit
        private int _powerUpCount;
        private Transform _transform;
        private static WaitForSeconds _autoAttackCheckDelay = new WaitForSeconds(0.1f);

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

            playerStats = Resources.Load<PlayerStats>("scriptableObjects/ClassesStats/" + playerClassType + "Stats");

            _currentHp = playerStats.health;
            _levelUpManager = FindFirstObjectByType<LevelUpManager>();
            _levelUpUI = FindFirstObjectByType<LevelUpUI>();
            _mainCamera = Camera.main;
            _transform = transform;

            if (_autoAttackEnabled)
            {
                StartCoroutine(AutoAttackCoroutine());
            }
        }

        private void Update()
        {
            // Only iterate actual power-ups, not capacity
            for (int i = 0; i < _powerUpCount; i++)
            {
                powerUps[i].Activate(this);
            }
        }
        
        private void UpdateTargetDirection()
        {
            if (!_autoTargetEnabled)
            {
                Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _manualTargetDirection = (mousePosition - transform.position).normalized;
            }
        }

        public void AddPowerUp(PowerUp powerUp)
        {
            if (_powerUpCount < MaxPowerUps)
            {
                powerUps.Add(powerUp);
                _powerUpCount++;
            }
        }

        // Optimize target finding with distance check first
        private Vector2 GetTargetDirection()
        {
            if (_autoTargetEnabled)
            {
                Enemy nearestEnemy = CombatUtilities.GetNearestEnemy(_transform.position, _autoAttackRange);
                if (nearestEnemy)
                {
                    Vector2 direction = (Vector2)nearestEnemy.transform.position - (Vector2)_transform.position;
                    float sqrDistance = direction.sqrMagnitude;
                    if (sqrDistance > 0.01f) // Avoid division by zero
                    {
                        return direction / Mathf.Sqrt(sqrDistance); // Manual normalization
                    }
                }
            }

            return _manualTargetDirection != Vector2.zero ? _manualTargetDirection : Vector2.right;
        }

        public void SetManualTargetDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                _manualTargetDirection = direction;
            }
        }

        public Player(PlayerStats playerStats, IClass playerClass)
        {
            this.playerStats = playerStats;
            _playerClass = playerClass;
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
            Mathf.Clamp01((Time.time - _lastSpecialAttackTime) /
                          (playerStats.specialAttackSpeed * statsMod.sAttackSpeedMod));

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
        
        private IEnumerator AutoAttackCoroutine()
        {
            while (true)
            {
                yield return _autoAttackCheckDelay;
        
                if (!_autoAttackEnabled) continue;
        
                UpdateTargetDirection();
                _vec = new Vec
                {
                    Position = _transform.position,
                    Direction = GetTargetDirection()
                };

                if (CanAttack())
                {
                    BasicAttack();
                }

                if (CanSpecialAttack())
                {
                    SpecialAttack();
                }
            }
        }
    }
}