using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.WarriorClass
{
    public class Berserker : IClass
    {
        private int _killStreak;
        private float _lastKillTime;
        private float _rageStacksEndTime;
        private int _rageStacks;
        private Player _player;
        private float _baseAttackSpeed;
        private MultiAttackController _multiAttackController;

        public void Initialize(Player player)
        {
            _player = player;
            _baseAttackSpeed = player.statsMod.attackSpeedMod;
            _multiAttackController = player.gameObject.GetComponent<MultiAttackController>();
            
            if (!_multiAttackController)
            {
                _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
            }
        }

        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            UpdateRageStacks(statsMod);

            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + statsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;
            float rageBonus = 1f + (_rageStacks * 0.1f);

            _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) =>
                        PerformSingleBasicAttack(playerStats, statsMod, vec, angleOffset, rageBonus),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleBasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec,
            float angleOffset, float rageBonus)
        {
            float enhancedArea = playerStats.Area * statsMod.areaMod * 1.3f;
            float range = playerStats.Range * statsMod.rangeMod;

            Vector2 direction = RotateVector(vec.Direction, angleOffset * 15f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, direction, range, enhancedArea);

            CombatEvents.TriggerAttack(vec.Position, direction, range, enhancedArea, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.Knockback * 0.2f * statsMod.knockbackMod * rageBonus);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

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

            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + Mathf.FloorToInt(statsMod.projectileCountBonus * 0.5f);
            float intervalBetweenAttacks = 0.12f;

            _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) => PerformSingleSpecialAttack(playerStats, statsMod, vec, angleOffset),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleSpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec,
            float angleOffset)
        {
            float radius = playerStats.Range * statsMod.rangeMod * 1.2f * (1f + angleOffset * 0.1f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);

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
            float radius = playerStats.Range * statsMod.rangeMod * 0.8f;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerStats.Knockback * statsMod.knockbackMod * 0.5f);
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

            if (previousStacks != _rageStacks)
            {
                ApplyRageBuffs(statsMod);
            }
        }

        private void ApplyRageBuffs(PlayerStatsModifier statsMod)
        {
            if (_baseAttackSpeed > 0)
            {
                statsMod.attackSpeedMod = _baseAttackSpeed;
            }
            else
            {
                _baseAttackSpeed = statsMod.attackSpeedMod;
            }

            float attackSpeedBonus = 1f + (_rageStacks * 0.1f);
            statsMod.attackSpeedMod = _baseAttackSpeed * attackSpeedBonus;

            if (_rageStacks == 0)
            {
                Debug.Log("Rage ended - attack speed returned to normal");
            }
        }

        public int GetRageStacks() => _rageStacks;

        private Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
    }
}