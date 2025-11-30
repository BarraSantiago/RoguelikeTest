using PlayerDirectory;

namespace ClassesDirectory
{
    public enum Classes
    {
        Warrior,
        Ranger
    }
    public interface IClass
    {
        public void Initialize(Player player);
        public void BasicAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public void SpecialAttack(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
        public void Defense(PlayerStats playerStats, PlayerStatsModifier statsMod, Vec vec);
    }
}