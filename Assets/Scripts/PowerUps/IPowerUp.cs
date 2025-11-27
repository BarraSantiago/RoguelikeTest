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
        [SerializeField] protected StatsModifier powerUpStatsMod;
        [SerializeField] protected Stats powerUpStats;
        [SerializeField] protected PowerUpLevel powerUpLevel = new PowerUpLevel();

        private float _lastAttackTime;
        private StatsModifier _baseStatsMod;
        private Stats _baseStats;

        public Stats PowerUpStats => powerUpStats;
        public StatsModifier PowerUpStatsMod => powerUpStatsMod;
        public PowerUpLevel Level => powerUpLevel;

        public virtual void OnPickup(Player player)
        {
            // Store base values
            _baseStats = new Stats();
            _baseStatsMod = new StatsModifier();

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
            return Time.time >= _lastAttackTime + PowerUpStats.attackSpeed;
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
            powerUpStats.damage = _baseStats.damage * (1 + powerUpLevel.damageIncrease * (powerUpLevel.level - 1));
            powerUpStats.attackSpeed = _baseStats.attackSpeed *
                                       (1 - powerUpLevel.attackSpeedDecrease * (powerUpLevel.level - 1));
            powerUpStats.range = _baseStats.range * (1 + powerUpLevel.rangeIncrease * (powerUpLevel.level - 1));
        }
    }
}