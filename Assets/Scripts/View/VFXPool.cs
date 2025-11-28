using Entities;
using UnityEngine;

namespace View
{
    public class VFXPool : GenericPool<VFXInstance>
    {
        public static VFXPool Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            base.Awake();
        }

        protected override void LoadPrefab()
        {
            GameObject obj = new GameObject("VFXInstance");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<VFXInstance>();
            obj.SetActive(false);
            prefab = obj;
        }
    }
}