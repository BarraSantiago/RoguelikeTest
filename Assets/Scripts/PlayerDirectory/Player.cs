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
        [SerializeField] private string _currentSubclass = "None";

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
        private bool _isInvincible;
        private Coroutine _healthRegenCoroutine;
        public Classes GetPlayerClassType() => playerClassType;

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
            
            playerStats.maxHealth = playerStats.health;
            _currentHp = playerStats.maxHealth;
        
            // Start health regeneration
            if (playerStats.healthRegen > 0)
            {
                _healthRegenCoroutine = StartCoroutine(HealthRegeneration());
            }
            
            StartCoroutine(DetectPickups());

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

        private bool CanDefense()
        {
            return Time.time >= _lastDefenseTime + playerStats.defenseSpeed * statsMod.defenseSpeedMod;
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
        
        private IEnumerator HealthRegeneration()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        
        while (true)
        {
            yield return wait;
            
            float totalRegen = (playerStats.healthRegen + statsMod.healthRegenBonus);
            if (totalRegen > 0 && _currentHp < GetMaxHealth())
            {
                Heal(totalRegen);
            }
        }
    }

    // Pickup magnet system
    private IEnumerator DetectPickups()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        
        while (true)
        {
            yield return wait;
            
            float radius = playerStats.pickupRadius * statsMod.pickupRadiusMod;
            Collider2D[] pickups = Physics2D.OverlapCircleAll(_transform.position, radius);
            
            foreach (Collider2D pickup in pickups)
            {
                if (pickup.CompareTag("Experience") || pickup.CompareTag("Coin"))
                {
                    AttractPickup(pickup.transform);
                }
            }
        }
    }

    private void AttractPickup(Transform pickup)
    {
        float magnetForce = playerStats.magnetism * statsMod.magnetismMod;
        Vector2 direction = (_transform.position - pickup.position).normalized;
        pickup.position = Vector2.MoveTowards(pickup.position, _transform.position, 
            magnetForce * 5f * Time.deltaTime);
    }

    public void TakeDamage(float attackDamage)
    {
        if (_isInvincible) return;
        
        // Apply armor reduction
        float totalArmor = playerStats.armor + statsMod.armorBonus;
        float finalDamage = Mathf.Max(0, (attackDamage - totalArmor) / (playerStats.defense * statsMod.defenseMod));
        
        _currentHp -= finalDamage;

        if (_currentHp <= 0)
        {
            AttemptRevival();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        _isInvincible = true;
        float duration = playerStats.invincibilityDuration;
        
        // Visual feedback
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        sprite.enabled = true;
        _isInvincible = false;
    }

    private void AttemptRevival()
    {
        int totalRevivals = playerStats.revival + statsMod.revivalBonus;
        
        if (totalRevivals > 0)
        {
            playerStats.revival--;
            _currentHp = GetMaxHealth() * 0.5f; // Revive with 50% HP
            StartCoroutine(InvincibilityFrames());
            Debug.Log($"Revived! {playerStats.revival} revivals remaining");
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        Time.timeScale = 0;
        // TODO: Show game over screen
    }

    public void Heal(float amount)
    {
        _currentHp = Mathf.Min(_currentHp + amount, GetMaxHealth());
    }

    public void GainExperience(float amount)
    {
        float totalGrowth = playerStats.growth * statsMod.growthMod;
        bool leveledUp = levelSystem.AddExperience(amount * totalGrowth);

        if (leveledUp)
        {
            OnLevelUp();
        }
    }

    private void OnLevelUp()
    {
        List<object> choices = _levelUpManager.GenerateLevelUpChoices(this);
        
        // Apply reroll/skip/banish charges
        int rerolls = playerStats.reroll + statsMod.rerollBonus;
        int skips = playerStats.skip + statsMod.skipBonus;
        int banishes = playerStats.banish + statsMod.banishBonus;
        
        _levelUpUI.ShowLevelUpChoices(this, choices, rerolls, skips, banishes);
    }

    // Calculate final cooldown with reduction
    private float GetModifiedCooldown(float baseCooldown, float cooldownMod)
    {
        float totalReduction = playerStats.cooldownReduction + statsMod.cooldownReductionBonus;
        return baseCooldown * cooldownMod * (1f - Mathf.Clamp01(totalReduction));
    }

    private bool CanAttack()
    {
        float modifiedCooldown = GetModifiedCooldown(playerStats.attackSpeed, statsMod.attackSpeedMod);
        return Time.time >= _lastAttackTime + modifiedCooldown;
    }

    private bool CanSpecialAttack()
    {
        float modifiedCooldown = GetModifiedCooldown(playerStats.specialAttackSpeed, statsMod.sAttackSpeedMod);
        return Time.time >= _lastSpecialAttackTime + modifiedCooldown;
    }

    // Getters for UI
    public float GetMaxHealth() => playerStats.maxHealth;
    public float GetCurrentHealth() => _currentHp;
    public float GetHealthPercentage() => _currentHp / GetMaxHealth();
    public int GetRevivals() => playerStats.revival + statsMod.revivalBonus;
    public float GetPickupRadius() => playerStats.pickupRadius * statsMod.pickupRadiusMod;

    }
}