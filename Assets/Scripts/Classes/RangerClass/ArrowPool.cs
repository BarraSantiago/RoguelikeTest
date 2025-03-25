using System.Collections.Generic;
using UnityEngine;

namespace Classes.RangerClass
{
    public class ArrowPool : MonoBehaviour
    {
        public GameObject arrowPrefab;
        public int poolSize = 10;
        private Queue<GameObject> arrowPool;

        private void Awake()
        {
            arrowPool = new Queue<GameObject>();
            if (!arrowPrefab)
            {
                arrowPrefab = Resources.Load<GameObject>("Prefabs/Arrow");
            }
            if(!arrowPrefab) return;
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