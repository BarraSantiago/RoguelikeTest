using System;
using PlayerDirectory;
using UnityEngine;

namespace PowerUps
{
    public interface IPowerUp
    {
        public void OnPickup(Player player);
        public void Activate(Player player);
        public bool CanActivate();
    }

    [Serializable]
    public class PowerUpLevel
    {
        public int level = 1;
        public int maxLevel = 5;
        public float damageIncrease = 0.2f;
        public float attackSpeedDecrease = 0.1f;
        public float rangeIncrease = 0.15f;
    }

    public abstract class PowerUp : ScriptableObject, IPowerUp
    {
        [SerializeField] protected BaseStatsMod powerUpBaseStatsMod;
        [SerializeField] protected BaseStats _powerUpBaseStats;
        [SerializeField] protected PowerUpLevel powerUpLevel = new PowerUpLevel();

        private float _lastAttackTime;
        private BaseStatsMod _baseBaseStatsMod;
        private BaseStats _baseBaseStats;

        public BaseStats PowerUpBaseStats => _powerUpBaseStats;
        public BaseStatsMod PowerUpBaseStatsMod => powerUpBaseStatsMod;
        public PowerUpLevel Level => powerUpLevel;

        public virtual void OnPickup(Player player)
        {
            // Store base values
            _baseBaseStats = new BaseStats();
            _baseBaseStatsMod = new BaseStatsMod();

            player.powerUps.Add(this);
            _lastAttackTime = Time.time;
        }

        public virtual void Activate(Player player)
        {
            if (!CanActivate()) return;
            _lastAttackTime = Time.time;
        }

        public virtual bool CanActivate()
        {
            return Time.time >= _lastAttackTime + PowerUpBaseStats.AttackSpeed;
        }

        public virtual bool CanUpgrade()
        {
            return powerUpLevel.level < powerUpLevel.maxLevel;
        }

        public virtual void Upgrade()
        {
            if (!CanUpgrade()) return;

            powerUpLevel.level++;

            // Apply stat increases
            _powerUpBaseStats.Damage = _baseBaseStats.Damage * (1 + powerUpLevel.damageIncrease * (powerUpLevel.level - 1));
            _powerUpBaseStats.AttackSpeed = _baseBaseStats.AttackSpeed *
                                       (1 - powerUpLevel.attackSpeedDecrease * (powerUpLevel.level - 1));
            _powerUpBaseStats.Range = _baseBaseStats.Range * (1 + powerUpLevel.rangeIncrease * (powerUpLevel.level - 1));
        }
    }
}