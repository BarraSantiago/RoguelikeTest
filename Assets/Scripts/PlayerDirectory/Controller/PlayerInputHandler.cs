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

        private void Awake()
        {
            _player = GetComponent<Player>();
            _pauseManager = FindObjectOfType<PauseManager>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _player.BasicAttack();
            }
        }

        public void OnSpecialAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _player.SpecialAttack();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _player.Defense();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>();
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
            if (_moveDirection != Vector2.zero)
            {
                _player.Move(_moveDirection);
            }
        }
    }
}