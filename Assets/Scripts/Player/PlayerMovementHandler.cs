using DG.Tweening;
using Managers;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerMovementHandler : MonoBehaviour
    {
        [Header("Jump")] [SerializeField] private float jumpForce = 14f;

        [Header("Forgiveness")] [SerializeField]
        private float coyoteTime = 0.15f;

        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Better Jump")] [SerializeField]
        private float fallMultiplier = 2.5f;

        [SerializeField] private float lowJumpMultiplier = 2f;

        [Header("Apex Hang")] [SerializeField] private float apexThreshold = 1.5f;

        [SerializeField] private float apexHangMultiplier = 0.5f;

        [Header("Hitboxes")] [SerializeField] private Collider2D hitbox;
        [SerializeField] private Collider2D hitboxCrouch;

        [Header("Ground Check")] [SerializeField]
        private Transform groundCheck;

        [SerializeField] private float groundRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Fast Fall")] [SerializeField] private float fastFallMultiplier = 4f; // ← tweak in inspector


        [Header("Debug")] [SerializeField] private bool showDebugLogs;


        private Animator _animator;

        private float _coyoteTimer;

        private PlayerHealthComponent _healthComponent;
        private PlayerInputHandler _input;

        private bool _isCrouching;

        private bool _isFastFalling;

        private bool _isFixingPosition;
        private bool _isGrounded;
        private float _jumpBufferTimer;

        private Rigidbody2D _rb;
        private bool _wasGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputHandler>();
            _healthComponent = GetComponent<PlayerHealthComponent>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            UpdateGroundState();
            UpdateTimers();
            HandleJumpInput();
            HandleCrouchInput();
            FixPosition();
        }

        private void FixedUpdate()
        {
            ApplyBetterJumpPhysics();
        }

        private void OnDrawGizmos()
        {
            if (groundCheck == null)
                return;

            bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        private void FixPosition()
        {
            if (_isFixingPosition
                || Mathf.Approximately(transform.position.x, -8f)
                || !_isGrounded
                || GameManager.Instance.CurrentGameState != Data.GameState.Playing) return;
            _isFixingPosition = true;
            transform.DOMoveX(-8f, 5f)
                .SetEase(Ease.OutSine)
                .OnComplete(() => _isFixingPosition = false);
        }

        private void UpdateGroundState()
        {
            _wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

            _animator.SetBool("isGrounded", _isGrounded);

            if (_isGrounded) _coyoteTimer = coyoteTime;
            
            if (showDebugLogs && _isGrounded != _wasGrounded) Debug.Log(_isGrounded ? "Grounded" : "Airborne");
        }

        private void UpdateTimers()
        {
            if (!_isGrounded) _coyoteTimer -= Time.deltaTime;

            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;
        }

        private void HandleJumpInput()
        {
            if (_input.JumpPressed)
            {
                _jumpBufferTimer = jumpBufferTime;
                _input.ConsumeJumpPressed();
                _animator.SetTrigger("Jump");
            }

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f) PerformJump();
        }

        private void PerformJump()
        {
            var velocity = _rb.linearVelocity;
            velocity.y = jumpForce;
            _rb.linearVelocity = velocity;

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            AudioManager.Instance.Request(Data.SoundId.PlayerJump);

            if (showDebugLogs) Debug.Log("Jump executed");
        }

        private void FastFall()
        {
            _isFastFalling = true;
        }

        private void ApplyBetterJumpPhysics()
        {
            if (!_healthComponent.IsAlive) return;

            var yVelocity = _rb.linearVelocity.y;

            if (_isFastFalling)
            {
                _rb.linearVelocity +=
                    Vector2.up * (Physics2D.gravity.y * (fastFallMultiplier - 1f) * Time.fixedDeltaTime);

                if (_isGrounded)
                    _isFastFalling = false;

                return;
            }

            if (yVelocity < 0f)
            {
                var gravityMultiplier = fallMultiplier;
                if (Mathf.Abs(yVelocity) < apexThreshold) gravityMultiplier *= apexHangMultiplier;
                _rb.linearVelocity +=
                    Vector2.up * (Physics2D.gravity.y * (gravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (yVelocity > 0f)
            {
                if (!_input.JumpHeld)
                    _rb.linearVelocity +=
                        Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
                else if (yVelocity < apexThreshold)
                    _rb.linearVelocity += Vector2.up *
                                          (Physics2D.gravity.y * ((apexHangMultiplier - 1f) * Time.fixedDeltaTime));
            }
        }

        private void HandleCrouchInput()
        {
            if (_input.CrouchHeld && !_isGrounded)
            {
                FastFall();
                return;
            }

            if (!_isGrounded)
            {
                ExitCrouch();
                return;
            }

            if (_input.CrouchHeld && !_isCrouching)
                EnterCrouch();
            else if (!_input.CrouchHeld && _isCrouching)
                ExitCrouch();
        }

        private void EnterCrouch()
        {
            _isCrouching = true;
            hitbox.enabled = false;
            hitboxCrouch.enabled = true;
            _animator.SetBool("Crouch", true);
        }

        private void ExitCrouch()
        {
            if (!_isCrouching) return;
            _isCrouching = false;
            _isFastFalling = false; // ← reset
            hitbox.enabled = true;
            hitboxCrouch.enabled = false;
            _animator.SetBool("Crouch", false);
        }
    }
}