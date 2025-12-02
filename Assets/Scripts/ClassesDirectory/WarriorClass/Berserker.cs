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
            _baseAttackSpeed = player.baseStatsMod.attackSpeedMod;
            _multiAttackController = player.gameObject.GetComponent<MultiAttackController>();
            
            if (!_multiAttackController)
            {
                _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
            }
        }

        public void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            UpdateRageStacks(baseStatsMod);

            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + baseStatsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;
            float rageBonus = 1f + (_rageStacks * 0.1f);

            _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) =>
                        PerformSingleBasicAttack(playerBaseStats, baseStatsMod, vec, angleOffset, rageBonus),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleBasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec,
            float angleOffset, float rageBonus)
        {
            float enhancedArea = playerBaseStats.Area * baseStatsMod.areaMod * 1.3f;
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;

            Vector2 direction = RotateVector(vec.Direction, angleOffset * 15f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, direction, range, enhancedArea);

            CombatEvents.TriggerAttack(vec.Position, direction, range, enhancedArea, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerBaseStats.Knockback * 0.2f * baseStatsMod.knockbackMod * rageBonus);
            CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);

            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsDead())
                {
                    OnEnemyKilled(baseStatsMod);
                }
            }
        }

        public void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            UpdateRageStacks(baseStatsMod);

            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + Mathf.FloorToInt(baseStatsMod.projectileCountBonus * 0.5f);
            float intervalBetweenAttacks = 0.12f;

            _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) => PerformSingleSpecialAttack(playerBaseStats, baseStatsMod, vec, angleOffset),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleSpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec,
            float angleOffset)
        {
            float radius = playerBaseStats.Range * baseStatsMod.rangeMod * 1.2f * (1f + angleOffset * 0.1f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);

            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsDead())
                {
                    OnEnemyKilled(baseStatsMod);
                }
            }
        }

        public void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float radius = playerBaseStats.Range * baseStatsMod.rangeMod * 0.8f;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerBaseStats.Knockback * baseStatsMod.knockbackMod * 0.5f);
        }

        private void OnEnemyKilled(PlayerBaseStatsMod baseStatsMod)
        {
            _killStreak++;
            _lastKillTime = Time.time;

            if (_killStreak % 5 == 0)
            {
                _rageStacks++;
                _rageStacksEndTime = Time.time + 5f;

                ApplyRageBuffs(baseStatsMod);
                Debug.Log($"Rage Stack! Level: {_rageStacks} (+{_rageStacks * 10}% attack speed)");
            }
        }

        private void UpdateRageStacks(PlayerBaseStatsMod baseStatsMod)
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
                ApplyRageBuffs(baseStatsMod);
            }
        }

        private void ApplyRageBuffs(PlayerBaseStatsMod baseStatsMod)
        {
            if (_baseAttackSpeed > 0)
            {
                baseStatsMod.attackSpeedMod = _baseAttackSpeed;
            }
            else
            {
                _baseAttackSpeed = baseStatsMod.attackSpeedMod;
            }

            float attackSpeedBonus = 1f + (_rageStacks * 0.1f);
            baseStatsMod.attackSpeedMod = _baseAttackSpeed * attackSpeedBonus;

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