using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class GenericPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialPoolSize = 10;

        private Queue<T> _pool = new();

        protected virtual void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewInstance();
            }
        }

        private void CreateNewInstance()
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            T component = obj.GetComponent<T>();
            if (component)
            {
                _pool.Enqueue(component);
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
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }
}