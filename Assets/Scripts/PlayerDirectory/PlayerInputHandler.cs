using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerDirectory
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private Player player;
        private Vector2 moveDirection;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                player.BasicAttack();
            }
        }

        public void OnSpecialAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                player.SpecialAttack();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                player.Defense();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
        
        private void Update()
        {
            if (moveDirection != Vector2.zero)
            {
                player.Move(moveDirection);
            }
        }
    }
}