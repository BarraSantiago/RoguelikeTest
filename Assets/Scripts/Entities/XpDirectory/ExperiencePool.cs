using PlayerDirectory;
using UnityEngine;

namespace Entities.XpDirectory
{
    public class ExperiencePool : GenericPool<ExperienceDrop>
    {
        protected override void Awake()
        {
            base.Awake();
            ExperienceDrop.Player = FindFirstObjectByType<Player>();
        }

        public void SpawnExperienceDrop(Vector3 position, float experienceAmount)
        {
            ExperienceDrop expDrop = Get();
            expDrop.Initialize(position, experienceAmount, this);
        }
    }
}