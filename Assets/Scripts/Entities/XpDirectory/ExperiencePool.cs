using System.Collections.Generic;
using PlayerDirectory;
using UnityEngine;

namespace Entities.XpDirectory
{
    public class ExperiencePool : MonoBehaviour
    {
        [SerializeField] private GameObject experienceDropPrefab;
        [SerializeField] private int initialPoolSize = 50;

        private Queue<ExperienceDrop> pooledExperienceDrops = new Queue<ExperienceDrop>();

        private void Awake()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewExperienceDrop();
            }

            ExperienceDrop.player = FindFirstObjectByType<Player>();
        }

        private void CreateNewExperienceDrop()
        {
            GameObject obj = Instantiate(experienceDropPrefab, transform);
            obj.SetActive(false);
            ExperienceDrop expDrop = obj.GetComponent<ExperienceDrop>();
            pooledExperienceDrops.Enqueue(expDrop);
        }

        public ExperienceDrop GetExperienceDrop()
        {
            if (pooledExperienceDrops.Count == 0)
            {
                CreateNewExperienceDrop();
            }

            return pooledExperienceDrops.Dequeue();
        }

        public void ReturnExperienceDrop(ExperienceDrop experienceDrop)
        {
            pooledExperienceDrops.Enqueue(experienceDrop);
        }

        public void SpawnExperienceDrop(Vector3 position, float experienceAmount)
        {
            ExperienceDrop expDrop = GetExperienceDrop();
            expDrop.Initialize(position, experienceAmount, this);
        }
    }
}