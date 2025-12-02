using System;
using System.Collections;
using System.Collections.Generic;
using Achievements;
using ClassesDirectory;
using ClassesDirectory.Evolution;
using ClassesDirectory.RangerClass;
using ClassesDirectory.WarriorClass;
using Entities.EnemyDirectory;
using PlayerDirectory.LevelingSystem;
using PowerUps;
using UnityEngine;
using Utilities;
using View.UI;
using View.UI.Leveling;
using View.UI.Menus;

namespace PlayerDirectory
{
    public struct Vec
    {
        public Vector2 Position;
        public Vector2 Direction;
    }



    public class Player : MonoBehaviour
    {
        [SerializeField] private Classes playerClassType;
        [SerializeField] private bool autoTargetEnabled = true;
        [SerializeField] private bool autoAttackEnabled = true;
        [SerializeField] private float autoAttackRange = 10f;
        [SerializeField] private string currentSubclass = "None";

        public Classes GetPlayerClassType() => playerClassType;
        public PlayerBaseStats PlayerBaseStats;
        public PlayerBaseStatsMod baseStatsMod;
        public List<PowerUp> powerUps;
        public LevelSystem levelSystem = new();
        public GameSessionStats sessionStats = new();
        
        private DeathScreenUI _deathScreenUI;
        private Vector2 _lastPosition;
        private bool Alive => _currentHp > 0;
        private static WaitForSeconds _autoAttackCheckDelay = new(0.1f);
        private LevelUpManager _levelUpManager;
        private IClass _playerClass;
        private LevelUpUI _levelUpUI;
        private Coroutine _healthRegenCoroutine;
        private Vector2 _manualTargetDirection;
        private Transform _transform;
        private Vec _vec;
        private float _lastAttackTime;
        private float _lastSpecialAttackTime;
        private float _lastDefenseTime;
        private float _currentHp = 100;
        private static readonly int MaxPowerUps = 10; // Reasonable limit
        private int _powerUpCount;
        private bool _isInvincible;

        private void Awake()
        {
            playerClassType = LoadingSceneManager.SelectedClass;
            string selectedSubclass = LoadingSceneManager.SelectedSubclass;

            switch (playerClassType)
            {
                case Classes.Warrior:
                    _playerClass = new Warrior();
                    _playerClass.Initialize(this);
            
                    // Apply subclass if selected
                    if (selectedSubclass != "None")
                    {
                        ApplySubclass(selectedSubclass);
                    }
                    break;
                case Classes.Ranger:
                    _playerClass = new Ranger();
                    _playerClass.Initialize(this);
            
                    if (selectedSubclass != "None")
                    {
                        ApplySubclass(selectedSubclass);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PlayerBaseStats = new PlayerBaseStats(playerClassType);
            baseStatsMod = new PlayerBaseStatsMod();

            _currentHp = PlayerBaseStats.Health;
            _levelUpManager = FindFirstObjectByType<LevelUpManager>();
            _levelUpUI = FindFirstObjectByType<LevelUpUI>();
            _transform = transform;
            
            _deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
            sessionStats.Reset();
            _lastPosition = _transform.position;
            
            ApplyShopBonuses();
            
            PlayerBaseStats.MaxHealth = PlayerBaseStats.Health;
            _currentHp = PlayerBaseStats.MaxHealth;

            StartCoroutine(AutoAttackCoroutine());
            StartCoroutine(DetectPickups());
            StartCoroutine(HealthRegeneration());
        }

        private void Update()
        {
            // Only iterate actual power-ups, not capacity
            for (int i = 0; i < _powerUpCount; i++)
            {
                powerUps[i].Activate(this);
            }
            
            sessionStats.survivalTime = Time.time;
            float distThisFrame = Vector2.Distance(_transform.position, _lastPosition);
            sessionStats.totalDistanceTraveled += distThisFrame;
            _lastPosition = _transform.position;
        }

        public void Heal(float amount)
        {
            _currentHp = Mathf.Min(_currentHp + amount, GetMaxHealth());
        }

        public void GainExperience(float amount)
        {
            float totalGrowth = PlayerBaseStats.Growth * baseStatsMod.growthMod;
            bool leveledUp = levelSystem.AddExperience(amount * totalGrowth);

            if (leveledUp)
            {
                OnLevelUp();
            }
        }
        
        public void EvolveClass(IClass newClass, string subclassName)
        {
            _playerClass = newClass;
            currentSubclass = subclassName;

            _playerClass.Initialize(this);

            Debug.Log($"Evolved to {subclassName}!");
            StartCoroutine(EvolutionEffect());
        }
        public void OnDamageDealt(float damage, bool wasCrit)
        {
            sessionStats.totalDamageDealt += damage;
            if (wasCrit) sessionStats.criticalHits++;
        }

        public void AddPowerUp(PowerUp powerUp)
        {
            if (_powerUpCount < MaxPowerUps)
            {
                powerUps.Add(powerUp);
                _powerUpCount++;
            }
        }
        
        public void SetManualTargetDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                _manualTargetDirection = direction;
            }
        }

        public Player(PlayerBaseStats playerBaseStats, IClass playerClass)
        {
            this.PlayerBaseStats = playerBaseStats;
            _playerClass = playerClass;
            _lastAttackTime = -playerBaseStats.AttackSpeed;
            _lastSpecialAttackTime = -playerBaseStats.SpecialAttackSpeed;
            _lastDefenseTime = -playerBaseStats.DefenseSpeed;
        }

        public void BasicAttack()
        {
            if (!CanAttack()) return;
            _playerClass.BasicAttack(PlayerBaseStats, baseStatsMod, _vec);
            _lastAttackTime = Time.time;
        }

        public void SpecialAttack()
        {
            if (!CanSpecialAttack()) return;
            _playerClass.SpecialAttack(PlayerBaseStats, baseStatsMod, _vec);
            _lastSpecialAttackTime = Time.time;
        }

        public void Defense()
        {
            if (!CanDefense()) return;
            _playerClass.Defense(PlayerBaseStats, baseStatsMod, _vec);
            _lastDefenseTime = Time.time;
        }
        
        public void Move(Vector2 direction)
        {
            float speed = PlayerBaseStats.Speed * baseStatsMod.speedMod;
            _transform.position += (Vector3)(direction.normalized * (speed * Time.deltaTime));
        }
        
        private void ApplySubclass(string subclassName)
        {
            currentSubclass = subclassName;
    
            SubclassEvolutionManager evolutionManager = FindFirstObjectByType<SubclassEvolutionManager>();
            if (evolutionManager)
            {
                evolutionManager.EvolveToSubclass(subclassName);
            }
        }

        private IEnumerator AutoAttackCoroutine()
        {
            while (Alive)
            {
                yield return _autoAttackCheckDelay;
                
                Vector2 targetDirection = GetTargetDirection();
                
                // Only attack if we have a valid target
                if (targetDirection == Vector2.zero) continue;
                _vec = new Vec
                {
                    Position = _transform.position,
                    Direction = targetDirection
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

            while (Alive)
            {
                yield return wait;

                float totalRegen = (PlayerBaseStats.HealthRegen + baseStatsMod.healthRegenBonus);
                if (totalRegen > 0 && _currentHp < GetMaxHealth())
                {
                    Heal(totalRegen);
                }
            }
        }

        private IEnumerator DetectPickups()
        {
            WaitForSeconds wait = new WaitForSeconds(0.1f);

            while (Alive)
            {
                yield return wait;

                float radius = PlayerBaseStats.PickupRadius * baseStatsMod.pickupRadiusMod;
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
            float magnetForce = PlayerBaseStats.Magnetism * baseStatsMod.magnetismMod;
            Vector2 direction = (_transform.position - pickup.position).normalized;
            pickup.position = Vector2.MoveTowards(pickup.position, _transform.position,
                magnetForce * 5f * Time.deltaTime);
        }

        public void TakeDamage(float attackDamage)
        {
            if (_isInvincible) return;
        
            float totalArmor = PlayerBaseStats.Armor + baseStatsMod.armorBonus;
            float finalDamage = Mathf.Max(0, (attackDamage - totalArmor) / (PlayerBaseStats.Defense * baseStatsMod.defenseMod));
        
            _currentHp -= finalDamage;
            sessionStats.totalDamageTaken += finalDamage;
        
            VibrationManager.Instance?.VibrateLight();
        
            if (!Alive)
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
            float duration = PlayerBaseStats.InvincibilityDuration;

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
            int totalRevivals = PlayerBaseStats.Revival + baseStatsMod.revivalBonus;

            if (totalRevivals > 0)
            {
                PlayerBaseStats.Revival--;
                _currentHp = GetMaxHealth() * 0.5f;
                StartCoroutine(InvincibilityFrames());
                Debug.Log($"Revived! {PlayerBaseStats.Revival} revivals remaining");
            }
            else
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player died!");
            
            // Add heavy vibration on death
            VibrationManager.Instance?.VibrateHeavy();
            
            AchievementManager.Instance.OnSessionEnd(sessionStats);
            
            Time.timeScale = 0;
            SubclassEvolutionManager evolutionManager = FindFirstObjectByType<SubclassEvolutionManager>();
            _deathScreenUI.ShowDeathScreen(sessionStats, evolutionManager);
        }
        
        private void OnLevelUp()
        {
            List<object> choices = _levelUpManager.GenerateLevelUpChoices(this);

            // Apply reroll/skip/banish charges
            int rerolls = PlayerBaseStats.Reroll + baseStatsMod.rerollBonus;
            int skips = PlayerBaseStats.Skip + baseStatsMod.skipBonus;
            int banishes = PlayerBaseStats.Banish + baseStatsMod.banishBonus;

            _levelUpUI.ShowLevelUpChoices(this, choices, rerolls, skips, banishes);
        }

        private float GetModifiedCooldown(float baseCooldown, float cooldownMod)
        {
            float totalReduction = PlayerBaseStats.CooldownReduction + baseStatsMod.cooldownReductionBonus;
            return baseCooldown * cooldownMod * (1f - Mathf.Clamp01(totalReduction));
        }

        private bool CanAttack()
        {
            float modifiedCooldown = GetModifiedCooldown(PlayerBaseStats.AttackSpeed, baseStatsMod.attackSpeedMod);
            return Time.time >= _lastAttackTime + modifiedCooldown;
        }

        private bool CanSpecialAttack()
        {
            float modifiedCooldown = GetModifiedCooldown(PlayerBaseStats.SpecialAttackSpeed, baseStatsMod.sAttackSpeedMod);
            return Time.time >= _lastSpecialAttackTime + modifiedCooldown;
        }
        
        private bool CanDefense()
        {
            return Time.time >= _lastDefenseTime + PlayerBaseStats.DefenseSpeed * baseStatsMod.defenseSpeedMod;
        }

        private IEnumerator EvolutionEffect()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            Color original = sprite.color;

            for (int i = 0; i < 5; i++)
            {
                sprite.color = Color.cyan;
                yield return new WaitForSeconds(0.1f);
                sprite.color = original;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void ApplyShopBonuses()
        {
            Dictionary<StatBonusType, float> bonuses = ShopManager.Instance.GetPurchasedStatBonuses();
            
            foreach (KeyValuePair<StatBonusType, float> bonus in bonuses)
            {
                switch (bonus.Key)
                {
                    case StatBonusType.Growth:
                        baseStatsMod.growthMod += bonus.Value;
                        break;
                    case StatBonusType.Reroll:
                        PlayerBaseStats.Reroll += (int)bonus.Value;
                        break;
                    case StatBonusType.Skip:
                        PlayerBaseStats.Skip += (int)bonus.Value;
                        break;
                    case StatBonusType.Banish:
                        PlayerBaseStats.Banish += (int)bonus.Value;
                        break;
                    case StatBonusType.MaxHealth:
                        PlayerBaseStats.Health += bonus.Value;
                        PlayerBaseStats.MaxHealth += bonus.Value;
                        break;
                    case StatBonusType.Armor:
                        baseStatsMod.armorBonus += bonus.Value;
                        break;
                    case StatBonusType.HealthRegen:
                        baseStatsMod.healthRegenBonus += bonus.Value;
                        break;
                    case StatBonusType.PickupRadius:
                        baseStatsMod.pickupRadiusMod += bonus.Value;
                        break;
                    case StatBonusType.Luck:
                        baseStatsMod.luckMod += bonus.Value * 0.01f;
                        break;
                    case StatBonusType.CooldownReduction:
                        PlayerBaseStats.CooldownReduction += bonus.Value;
                        break;
                }
            }
        }
        
         // Getters and Setters 
         
                 
         private Vector2 GetTargetDirection()
         {
             Enemy nearestEnemy = CombatUtilities.GetNearestEnemy(_transform.position, autoAttackRange);
             if (nearestEnemy)
             {
                 Vector2 direction = (Vector2)nearestEnemy.transform.position - (Vector2)_transform.position;
                 float sqrDistance = direction.sqrMagnitude;
                 if (sqrDistance > 0.01f)
                 {
                     return direction / Mathf.Sqrt(sqrDistance);
                 }
             }
            
             return Vector2.zero;
         }

        public bool AutoTargetEnabled
        {
            get => autoTargetEnabled;
            set => autoTargetEnabled = value;
        }

        public bool AutoAttackEnabled
        {
            get => autoAttackEnabled;
            set => autoAttackEnabled = value;
        }
        
        public float SpecialAttackCooldownProgress =>
            Mathf.Clamp01((Time.time - _lastSpecialAttackTime) /
                          (PlayerBaseStats.SpecialAttackSpeed * baseStatsMod.sAttackSpeedMod));

        public float DefenseCooldownProgress =>
            Mathf.Clamp01((Time.time - _lastDefenseTime) / (PlayerBaseStats.DefenseSpeed * baseStatsMod.defenseSpeedMod));
        // Getters for UI
        public void OnCoinCollected() => sessionStats.totalCoinsCollected++;
        public void OnProjectileFired() => sessionStats.projectilesFired++;
        public string GetCurrentSubclass() => currentSubclass;
        public float GetMaxHealth() => PlayerBaseStats.MaxHealth;
        public float GetCurrentHealth() => _currentHp;
        public float GetHealthPercentage() => _currentHp / GetMaxHealth();
        public int GetRevivals() => PlayerBaseStats.Revival + baseStatsMod.revivalBonus;
        public float GetPickupRadius() => PlayerBaseStats.PickupRadius * baseStatsMod.pickupRadiusMod;
    }
}