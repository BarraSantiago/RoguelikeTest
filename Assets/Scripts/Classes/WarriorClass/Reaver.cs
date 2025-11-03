using PlayerDirectory;
using UnityEngine;
using Entities.EnemyDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Reaver : IClass
    {
        private int _enemiesKilledThisRun;
        private float _bonusMaxHpPercentage;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            CombatEvents.TriggerAttack(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod,
                AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * 0.2f * statsMod.knockbackMod);
            
            float damageDealt = CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
            ApplyLifesteal(damageDealt);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod, AttackType.WarriorSpecial);

            // Leech all enemies
            float totalDamage = CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
            ApplyLifesteal(totalDamage * 1.5f); // 50% bonus lifesteal on special
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * statsMod.knockbackMod);
        }

        private void ApplyLifesteal(float damageDealt)
        {
            float healAmount = damageDealt * 0.15f;
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            player.Heal(healAmount);
        }

        public void OnEnemyKilled()
        {
            _enemiesKilledThisRun++;

            if (_enemiesKilledThisRun % 25 == 0)
            {
                _bonusMaxHpPercentage += 0.005f;
                Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
                player.playerStats.maxHealth *= 1.005f;
                Debug.Log($"Max HP increased! Bonus: +{_bonusMaxHpPercentage * 100:F1}%");
            }
        }

        public float GetBonusMaxHp() => _bonusMaxHpPercentage;
    }
}