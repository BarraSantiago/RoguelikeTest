using UnityEngine;

namespace View.Camera
{
    public class CameraFollowPlayer : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
        [SerializeField] private bool useSmoothing = true;

        private void LateUpdate()
        {
            if (!target) return;

            Vector3 desiredPosition = target.position + offset;

            if (useSmoothing)
            {
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
            }
            else
            {
                transform.position = desiredPosition;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}