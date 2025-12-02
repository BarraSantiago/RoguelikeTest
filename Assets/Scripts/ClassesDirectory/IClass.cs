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
        public void BasicAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec);
        public void SpecialAttack(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec);
        public void Defense(PlayerBaseStats playerBaseStats, PlayerBaseStatsMod baseStatsMod, Vec vec);
    }
}