using PlayerDirectory;
using UnityEngine;
using Entities.EnemyDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Berserker : IClass
    {
        private int _killStreak;
        private float _lastKillTime;
        private float _rageStacksEndTime;
        private int _rageStacks;
        private Player _player;
        private float _baseAttackSpeed;

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            UpdateRageStacks(statsMod);

            // Apply rage bonus
            float rageBonus = 1f + (_rageStacks * 0.1f);
            float enhancedArea = playerStats.area * statsMod.areaMod * 1.3f;

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, enhancedArea);

            CombatEvents.TriggerAttack(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, enhancedArea, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * 0.2f * statsMod.knockbackMod * rageBonus);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

            // Track kills for rage
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsDead())
                {
                    OnEnemyKilled(statsMod);
                }
            }
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            UpdateRageStacks(statsMod);
            
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod * 1.2f);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod * 1.2f, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

            // Track kills from special
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsDead())
                {
                    OnEnemyKilled(statsMod);
                }
            }
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position,
                playerStats.range * statsMod.rangeMod * 0.8f);

            CombatEvents.TriggerRadialAttack(vec.Position,
                playerStats.range * statsMod.rangeMod * 0.8f, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.knockback * statsMod.knockbackMod * 0.5f);
        }

        private void OnEnemyKilled(PlayerStatsModifier statsMod)
        {
            _killStreak++;
            _lastKillTime = Time.time;

            if (_killStreak % 5 == 0)
            {
                _rageStacks++;
                _rageStacksEndTime = Time.time + 5f;

                ApplyRageBuffs(statsMod);
                Debug.Log($"Rage Stack! Level: {_rageStacks} (+{_rageStacks * 10}% attack speed)");
            }
        }

        private void UpdateRageStacks(PlayerStatsModifier statsMod)
        {
            int previousStacks = _rageStacks;

            if (Time.time > _rageStacksEndTime)
            {
                _rageStacks = 0;
                _killStreak = 0;
            }

            if (Time.time - _lastKillTime > 3f)
            {
                _killStreak = 0;
            }

            // If stacks changed, reapply buffs
            if (previousStacks != _rageStacks)
            {
                ApplyRageBuffs(statsMod);
            }
        }

        private void ApplyRageBuffs(PlayerStatsModifier statsMod)
        {
            // Remove previous buff
            if (_baseAttackSpeed > 0)
            {
                statsMod.attackSpeedMod = _baseAttackSpeed;
            }
            else
            {
                _baseAttackSpeed = statsMod.attackSpeedMod;
            }

            // Apply new buff (10% attack speed per stack)
            float attackSpeedBonus = 1f + (_rageStacks * 0.1f);
            statsMod.attackSpeedMod = _baseAttackSpeed * attackSpeedBonus;

            if (_rageStacks == 0)
            {
                Debug.Log("Rage ended - attack speed returned to normal");
            }
        }

        public int GetRageStacks() => _rageStacks;

        public void Initialize(Player player)
        {
            _player = player;
            _baseAttackSpeed = player.statsMod.attackSpeedMod;
        }
    }
}