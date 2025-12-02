using Entities.EnemyDirectory;
using PlayerDirectory;
using UnityEngine;
using Utilities;

namespace ClassesDirectory.WarriorClass
{
    public class Warrior : IClass
    {
        private MultiAttackController _multiAttackController;
        private Player _player;
        private Coroutine _currentAttackCoroutine;

        public void Initialize(Player player)
        {
            _player = player;
            _multiAttackController = player.GetComponent<MultiAttackController>();
            if (!_multiAttackController)
            {
                _multiAttackController = player.gameObject.AddComponent<MultiAttackController>();
            }
        }

        public void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + baseStatsMod.projectileCountBonus;
            float intervalBetweenAttacks = 0.08f;

            _currentAttackCoroutine = _player.StartCoroutine(
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

            // Apply angle offset to direction
            Vector2 direction = RotateVector(vec.Direction, angleOffset * 15f);

            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, direction, range, area);

            CombatEvents.TriggerAttack(vec.Position, direction, range, area, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position,
                playerBaseStats.Knockback * 0.2f * baseStatsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);
        }

        public void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            if (!_multiAttackController.CanAttack()) return;

            int attackCount = 1 + Mathf.FloorToInt(baseStatsMod.projectileCountBonus * 0.5f);
            float intervalBetweenAttacks = 0.12f;

            _currentAttackCoroutine = _player.StartCoroutine(
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

            CombatUtilities.DamageEnemies(enemies, playerBaseStats, baseStatsMod);
        }

        public void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec)
        {
            float radius = playerBaseStats.Range * baseStatsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, radius);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerBaseStats.Knockback * baseStatsMod.knockbackMod);
        }

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