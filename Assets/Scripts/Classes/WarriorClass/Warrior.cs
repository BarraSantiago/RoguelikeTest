using Entities.EnemyDirectory;
using PlayerDirectory;

namespace Classes.WarriorClass
{
    public class Warrior : PlayerClass
    {
        public override void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies = GetEnemiesInCone(direction.position, direction.direction,
                playerStats.range * statsMod.rangeMod, playerStats.area * statsMod.areaMod);

            KnockbackEnemies(enemies, direction.position, playerStats.knockback * 0.2f * statsMod.knockbackMod);
            DamageEnemies(enemies, playerStats, statsMod);
        }

        public override void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies = GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            DamageEnemies(enemies, playerStats, statsMod);
        }

        public override void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Direction direction)
        {
            Enemy[] enemies = GetEnemiesInRadius(direction.position, playerStats.range * statsMod.rangeMod);
            KnockbackEnemies(enemies, direction.position, playerStats.knockback * statsMod.knockbackMod);
        }
    }
}