using UnityEngine;
using PlayerDirectory;

namespace PlayerDirectory.Controller
{
    public class AimJoystick : MonoBehaviour
    {
        [SerializeField] private FixedJoystick aimJoystick;
        [SerializeField] private Player player;

        private void Awake()
        {
            if (!player)
            {
                player = FindFirstObjectByType<Player>();
            }

            if (!aimJoystick)
            {
                aimJoystick = GetComponent<FixedJoystick>();
            }
        }

        private void Update()
        {
            if (!player || !aimJoystick || player.AutoTargetEnabled) return;

            Vector2 aimDirection = new Vector2(aimJoystick.Horizontal, aimJoystick.Vertical);

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                player.SetManualTargetDirection(aimDirection.normalized);
            }
        }
    }
}