using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour, InputSystemActions.IPlayerActions
    {
        private InputSystemActions _inputActions;

        private Vector2 _moveInput;
        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _attackPressed;

        public Vector2 MoveInput => _moveInput;
        public bool JumpPressed => _jumpPressed;
        public bool JumpHeld => _jumpHeld;
        public bool AttackPressed => _attackPressed;

        private void Awake()
        {
            _inputActions = new InputSystemActions();
            _inputActions.Player.SetCallbacks(this);
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                _jumpPressed = true;
                _jumpHeld = true;
            }

            if (context.canceled)
            {
                _jumpHeld = false;
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
        }

        public void ConsumeJumpPressed()
        {
            _jumpPressed = false;
        }
    }
}