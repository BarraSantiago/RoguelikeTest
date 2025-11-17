using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class GenericPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected int initialPoolSize = 20;

        private Queue<T> _pool = new();

        protected virtual void Awake()
        {
            if (!prefab)
            {
                LoadPrefab();
            }

            InitializePool();
        }

        protected virtual void LoadPrefab()
        {
            // Override in derived classes to load specific prefabs
        }

        private void InitializePool()
        {
            if (!prefab)
            {
                Debug.LogError($"Prefab not assigned for {typeof(T).Name} pool!");
                return;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewInstance();
            }
        }

        private void CreateNewInstance()
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            T component = obj.GetComponent<T>();
            if (component)
            {
                _pool.Enqueue(component);
            }
            else
            {
                Debug.LogError($"Prefab doesn't have component {typeof(T).Name}!");
                Destroy(obj);
            }
        }

        public T Get()
        {
            if (_pool.Count == 0)
            {
                CreateNewInstance();
            }

            T instance = _pool.Dequeue();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Return(T instance)
        {
            if (!instance) return;

            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }
}