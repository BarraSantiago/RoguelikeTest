using UnityEngine;
using System;

namespace Utilities
{
    public static class CombatEvents
    {
        public static event Action<Vector2, Vector2, float, float, AttackType> OnAttackPerformed;
        public static event Action<Vector2, float, AttackType> OnRadialAttackPerformed;

        public static void TriggerAttack(Vector2 position, Vector2 direction, float range, float area, AttackType type)
        {
            OnAttackPerformed?.Invoke(position, direction, range, area, type);
        }

        public static void TriggerRadialAttack(Vector2 position, float radius, AttackType type)
        {
            OnRadialAttackPerformed?.Invoke(position, radius, type);
        }
    }

    public enum AttackType
    {
        WarriorBasic,
        WarriorSpecial,
        WarriorDefense,
        RangerBasic,
        RangerSpecial
    }
}