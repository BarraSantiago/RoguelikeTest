using Entities.EnemyDirectory;
using PlayerDirectory;
using Utilities;

namespace Classes.WarriorClass
{
    public class Warrior : IClass
    {
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float range = playerStats.range * statsMod.rangeMod;
            float area = playerStats.area * statsMod.areaMod;
            
            Enemy[] enemies = CombatUtilities.GetEnemiesInCone(vec.Position, vec.Direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            CombatEvents.TriggerAttack(vec.Position, vec.Direction, range, area, AttackType.WarriorBasic);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerStats.knockback * 0.2f * statsMod.knockbackMod);
            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, playerStats.range * statsMod.rangeMod);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorSpecial);

            CombatUtilities.DamageEnemies(enemies, playerStats, statsMod);
        }

        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec)
        {
            float radius = playerStats.range * statsMod.rangeMod;

            Enemy[] enemies = CombatUtilities.GetEnemiesInRadius(vec.Position, playerStats.range * statsMod.rangeMod);
            CombatEvents.TriggerRadialAttack(vec.Position, radius, AttackType.WarriorDefense);

            CombatUtilities.KnockbackEnemies(enemies, vec.Position, playerStats.knockback * statsMod.knockbackMod);
        }
    }
}