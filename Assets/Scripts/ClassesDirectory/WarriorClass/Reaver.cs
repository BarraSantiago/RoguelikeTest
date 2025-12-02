using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.WarriorClass
{
    public class Reaver : IClass
    {
        private int _enemiesKilledThisRun;
        private float _bonusMaxHpPercentage;
        private MultiAttackController _multiAttackController;
        private Player _player;
        private int attackCount = 1;
        public void Initialize(Player player)
        {
            _player = player;
            _multiAttackController = player.gameObject.GetComponent<MultiAttackController>();
            if (!_multiAttackController)
            {
                _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
            }
        }

        public void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            attackCount = 1 + baseStatsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;

            _player.StartCoroutine(
                _multiAttackController.ExecuteMultiAttack(
                    (attackIndex, angleOffset) => PerformSingleBasicAttack(playerBaseStats, baseStatsMod, vec, angleOffset),
                    attackCount,
                    intervalBetweenAttacks
                )
            );
        }

        private void PerformSingleBasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec,
            float angleOffset)
        {
            float range = playerBaseStats.Range * baseStatsMod.rangeMod;
            float area = playerBaseStats.Area * baseStatsMod.areaMod;

            Vector2 direction = RotateVector(vec.Direction, angleOffset * 15f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, direction, range, area);

            CombatEvents.TriggerAttack(vec.Position, direction, range, area, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerBaseStats.Knockback * 0.2f * baseStatsMod.knockbackMod);

            float damageDealt = CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);
            ApplyLifesteal(damageDealt / attackCount); // Divide by attack count to prevent overheal
        }

        public void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
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
            float radius = playerBaseStats.Range * baseStatsMod.rangeMod * (1f + angleOffset * 0.1f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorSpecial);

            float totalDamage = CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);
            ApplyLifesteal(totalDamage * 1.5f); // 50% bonus lifesteal on special
        }

        public void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float radius = playerBaseStats.Range * baseStatsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerBaseStats.Knockback * baseStatsMod.knockbackMod);
        }

        private void ApplyLifesteal(float damageDealt)
        {
            float healAmount = damageDealt * 0.15f;
            _player.Heal(healAmount);
        }

        public void OnEnemyKilled()
        {
            _enemiesKilledThisRun++;

            if (_enemiesKilledThisRun % 25 == 0)
            {
                _bonusMaxHpPercentage += 0.005f;
                _player.PlayerBaseStats.MaxHealth *= 1.005f;
                Debug.Log($"Max HP increased! Bonus: +{_bonusMaxHpPercentage * 100:F1}%");
            }
        }

        public float GetBonusMaxHp() => _bonusMaxHpPercentage;

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