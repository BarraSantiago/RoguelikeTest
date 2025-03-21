using System.Collections.Generic;
    using UnityEngine;
    
    namespace Entities
    {
        public class EnemyPool : MonoBehaviour
        {
            public GameObject enemyPrefab;
            public int poolSize = 10;
            private Queue<GameObject> pool;
    
            void Awake()
            {
                pool = new Queue<GameObject>();
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject enemy = Instantiate(enemyPrefab);
                    enemy.SetActive(false);
                    pool.Enqueue(enemy);
                }
            }
    
            public GameObject GetEnemy()
            {
                if (pool.Count > 0)
                {
                    GameObject enemy = pool.Dequeue();
                    enemy.SetActive(true);
                    return enemy;
                }
                else
                {
                    GameObject enemy = Instantiate(enemyPrefab);
                    return enemy;
                }
            }
    
            public void ReturnEnemy(GameObject enemy)
            {
                enemy.SetActive(false);
                pool.Enqueue(enemy);
            }
        }
    }