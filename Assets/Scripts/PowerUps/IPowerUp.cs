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
    public abstract class PowerUp : ScriptableObject, IPowerUp
    {
        [SerializeField] protected StatsModifier powerUpStatsMod;
        [SerializeField] protected Stats powerUpStats;
        
        private float _lastAttackTime;

        public Stats PowerUpStats 
        { 
            get => powerUpStats;
            set => powerUpStats = value;
        }
        public StatsModifier PowerUpStatsMod 
        { 
            get => powerUpStatsMod;
            set => powerUpStatsMod = value;
        }

        public virtual void OnPickup(Player player)
        {
            player.powerUps.Add(this);
            _lastAttackTime = Time.time;
        }

        public virtual void Activate(Player player)
        {
            if(!CanActivate()) return;
            _lastAttackTime = Time.time;
        }

        public virtual bool CanActivate()
        {
            return Time.time >= _lastAttackTime + PowerUpStats.attackSpeed;
        }
    }
}
