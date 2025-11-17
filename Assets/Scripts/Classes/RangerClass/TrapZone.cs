using System.Collections.Generic;
using Entities.EnemyDirectory;
using UnityEngine;

namespace Classes.RangerClass
{
    public class TrapZone : MonoBehaviour
    {
        private float _radius;
        private float _duration;
        private bool _isRooting;
        private float _endTime;
        private List<Enemy> _affectedEnemies = new List<Enemy>();

        public void Initialize(float radius, float duration, bool isRooting)
        {
            _radius = radius;
            _duration = duration;
            _isRooting = isRooting;
            _endTime = Time.time + duration;

            // Visual effect
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.parent = transform;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(_radius * 2, 0.1f, _radius * 2);
            visual.GetComponent<Renderer>().material.color = new Color(0.5f, 0, 0.5f, 0.3f);
            Destroy(visual.GetComponent<Collider>());

            InvokeRepeating(nameof(ApplyTrapEffect), 0f, 0.2f);
        }

        private void ApplyTrapEffect()
        {
            if (Time.time >= _endTime)
            {
                Destroy(gameObject);
                return;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radius);

            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (_isRooting)
                    {
                        enemy.Root(_duration);
                    }
                    else
                    {
                        enemy.Slow(0.7f, _duration);
                    }
                }
            }
        }
    }
}