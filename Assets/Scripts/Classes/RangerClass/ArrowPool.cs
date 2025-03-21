using System.Collections.Generic;
using UnityEngine;

namespace Classes
{
    public class ArrowPool : MonoBehaviour
    {
        public GameObject arrowPrefab;
        public int poolSize = 10;
        private Queue<GameObject> arrowPool;

        void Awake()
        {
            arrowPool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject arrow = Instantiate(arrowPrefab);
                arrow.SetActive(false);
                arrowPool.Enqueue(arrow);
            }
        }

        public GameObject GetArrow()
        {
            if (arrowPool.Count > 0)
            {
                GameObject arrow = arrowPool.Dequeue();
                arrow.SetActive(true);
                return arrow;
            }
            else
            {
                GameObject arrow = Instantiate(arrowPrefab);
                return arrow;
            }
        }

        public void ReturnArrow(GameObject arrow)
        {
            arrow.SetActive(false);
            arrowPool.Enqueue(arrow);
        }
    }
}