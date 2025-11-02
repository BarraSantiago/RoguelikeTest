using UnityEngine;
using UnityEngine.InputSystem;
using View.UI.Menus;

namespace PlayerDirectory.Controller
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private Player _player;
        private Vector2 _moveDirection;
        private PauseManager _pauseManager;

        [SerializeField] private FixedJoystick moveJoystick;
        [SerializeField] private bool useJoystick = true;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _pauseManager = FindFirstObjectByType<PauseManager>();

            if (!moveJoystick)
            {
                moveJoystick = FindFirstObjectByType<FixedJoystick>();
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_pauseManager.IsPaused && !_player.AutoAttackEnabled)
            {
                _player.BasicAttack();
            }
        }

        public void OnSpecialAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_pauseManager.IsPaused && !_player.AutoAttackEnabled)
            {
                _player.SpecialAttack();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && !_pauseManager.IsPaused)
            {
                _player.Defense();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!useJoystick)
            {
                _moveDirection = context.ReadValue<Vector2>();
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed && _pauseManager)
            {
                _pauseManager.TogglePause();
            }
        }

        private void Update()
        {
            if (_pauseManager && _pauseManager.IsPaused) return;

            if (useJoystick && moveJoystick)
            {
                _moveDirection = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
            }

            if (_moveDirection != Vector2.zero)
            {
                _player.Move(_moveDirection);
            }
        }
    }
}