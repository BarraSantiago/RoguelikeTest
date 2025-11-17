using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Classes.RangerClass
{
    public class ArrowPool : GenericPool<Arrow>
    {
        private static ArrowPool _instance;
        private List<Arrow> _activeArrows = new();

        public static ArrowPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ArrowPool>();
                }

                return _instance;
            }
        }

        protected override void Awake()
        {
            _instance = this;
            base.Awake();
            Arrow._pool = this;
        }

        protected override void LoadPrefab()
        {
            base.LoadPrefab();
            prefab = Resources.Load<GameObject>("Prefabs/Arrow");
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            for (int i = _activeArrows.Count - 1; i >= 0; i--)
            {
                Arrow arrow = _activeArrows[i];
                if (!arrow || !arrow.gameObject.activeInHierarchy)
                {
                    _activeArrows.RemoveAt(i);
                    continue;
                }

                UpdateArrow(arrow, deltaTime);
            }
        }

        private void UpdateArrow(Arrow arrow, float deltaTime)
        {
            // Check range
            if (Vector2.Distance(arrow.StartPosition, arrow.transform.position) >= arrow.range)
            {
                arrow.OnArrowExpire();
                return;
            }

            // Update homing
            if (arrow.isHoming)
            {
                UpdateHoming(arrow, deltaTime);
            }

            // Move arrow
            arrow.transform.position += (Vector3)(arrow.Velocity * deltaTime);
        }

        private void UpdateHoming(Arrow arrow, float deltaTime)
        {
            if (!arrow.NearestEnemy || !arrow.NearestEnemy.gameObject.activeInHierarchy)
            {
                arrow.FindNearestEnemy();
            }

            if (arrow.NearestEnemy)
            {
                Vector2 direction = ((Vector2)arrow.NearestEnemy.position - (Vector2)arrow.transform.position)
                    .normalized;
                Vector2 newVelocity = Vector2.Lerp(arrow.Velocity, direction * arrow.Velocity.magnitude,
                    arrow.homingStrength * deltaTime);

                arrow.Velocity = newVelocity;

                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        public Arrow GetArrowWithSetup(Vector2 position, Vector2 velocity, float damage, float range,
            bool isPiercing = false)
        {
            Arrow arrow = Get();
            arrow.Setup(position, velocity, damage, range, isPiercing);
            _activeArrows.Add(arrow);
            return arrow;
        }

        public new void Return(Arrow arrow)
        {
            _activeArrows.Remove(arrow);
            base.Return(arrow);
        }

        private void OnDestroy()
        {
            _instance = null;
            Arrow._pool = null;
        }
    }
}