using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    // Movement-related fields
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpImpulse = 8f;
    public float airWalkSpeed = 3f;
    public float maxVerticalSpeed = 10f;  // New field to limit vertical speed
    private bool wasRunningAtJump;
    private bool runInputWhileAirborne;
    private Vector2 moveInput;

    // Audio-related fields
    public AudioSource footstepAudio;
    public float walkPitch = 0.8f;
    public float runPitch = 1.3f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;
    private Damageable damageable;
    private Collider2D playerCollider;

    // Player state flags
    private bool _isMoving;
    private bool _isRunning;
    private bool _isFacingRight = true;

    // Properties
    public bool IsMoving
    {
        get => _isMoving;
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool IsFacingRight
    {
        get => _isFacingRight;
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            }
            _isFacingRight = value;
        }
    }

    public bool CanMove => animator.GetBool(AnimationStrings.canMove) && PauseMenuScript.isPaused == false;
    public bool IsAlive => animator.GetBool(AnimationStrings.isAlive);

    public float CurrentMoveSpeed => (CanMove && IsMoving && !touchingDirections.IsOnWall)
        ? (touchingDirections.IsGrounded ? (IsRunning ? runSpeed : walkSpeed) : (wasRunningAtJump ? runSpeed : airWalkSpeed))
        : 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity)
        {
            HandleMovement();
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
        HandleFootsteps();
        HandleLanding();
    }

    // Handles the main movement logic
    private void HandleMovement()
    {
        if (touchingDirections.IsGrounded && touchingDirections.slopeAngle <= 60)
        {
            Vector2 adjustedMovement = CalculateSlopeMovement();
            float targetYVelocity = AdjustVerticalMovementForSlope();

            // Smoothly snap the player to the ground if needed
            SmoothSnapToGround(ref targetYVelocity);

            // Clamp the vertical velocity to prevent excessive speed in air
            targetYVelocity = Mathf.Clamp(targetYVelocity, -maxVerticalSpeed, maxVerticalSpeed);

            // Apply the adjusted velocity
            rb.velocity = new Vector2(adjustedMovement.x, targetYVelocity);
        }
        else
        {
            // Regular air movement with vertical velocity clamped
            rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, Mathf.Clamp(rb.velocity.y, -maxVerticalSpeed, maxVerticalSpeed));
        }
    }

    // Calculate player movement along the slope
    private Vector2 CalculateSlopeMovement()
    {
        Vector2 slopeNormal = touchingDirections.slopeNormal.normalized;
        Vector2 slopeParallel = new Vector2(slopeNormal.y, -slopeNormal.x);
        return slopeParallel * moveInput.x * CurrentMoveSpeed;
    }

    // Adjust vertical velocity when transitioning from a slope to flat ground
    private float AdjustVerticalMovementForSlope()
    {
        float targetYVelocity = rb.velocity.y;
        if (touchingDirections.slopeAngle < 5f)
        {
            targetYVelocity = Mathf.Lerp(rb.velocity.y, 0, 0.1f);
            rb.AddForce(Vector2.down * 20f, ForceMode2D.Force);

            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, touchingDirections.castFilter.layerMask);
            if (groundHit.collider != null && rb.velocity.y > 0)
            {
                targetYVelocity = 0;
            }
        }
        return targetYVelocity;
    }

    // Smoothly snaps the player to the ground if they are very close to it
    private void SmoothSnapToGround(ref float targetYVelocity)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, touchingDirections.castFilter.layerMask);
        if (hit.collider != null && rb.velocity.y <= 0)
        {
            float distanceToGround = hit.distance;

            // Use smooth transition if very close to the ground to avoid bouncing
            if (distanceToGround > 0.01f && distanceToGround < 0.5f)
            {
                float snapSpeed = Mathf.Clamp(distanceToGround * 10f, 0f, 10f); // Adjust speed based on distance
                rb.velocity = new Vector2(rb.velocity.x, -snapSpeed);
                targetYVelocity = Mathf.Lerp(targetYVelocity, 0, 0.1f); // Smooth out vertical velocity
            }
            else if (distanceToGround >= 0.5f)
            {
                // Hard snap when far from the ground
                transform.position = new Vector3(transform.position.x, transform.position.y - distanceToGround, transform.position.z);
                targetYVelocity = 0;
            }
        }
    }

    // Handles the player landing after jumping or falling
    private void HandleLanding()
    {
        if (!touchingDirections.WasGroundedLastFrame && touchingDirections.IsGrounded)
        {
            IsRunning = runInputWhileAirborne;
        }
    }

    // Play or stop footstep sound based on movement and grounded status
    private void HandleFootsteps()
    {
        if (IsMoving && touchingDirections.IsGrounded)
        {
            if (!footstepAudio.isPlaying)
            {
                footstepAudio.Play();
            }
            footstepAudio.pitch = IsRunning ? runPitch : walkPitch;
        }
        else if (footstepAudio.isPlaying)
        {
            footstepAudio.Stop();
        }
    }

    // Input callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (IsAlive && PauseMenuScript.isPaused == false)
        {
            IsMoving = moveInput != Vector2.zero;
            SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            runInputWhileAirborne = true;
            if (touchingDirections.IsGrounded)
            {
                IsRunning = true;
            }
        }
        else if (context.canceled)
        {
            runInputWhileAirborne = false;
            if (touchingDirections.IsGrounded)
            {
                IsRunning = false;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirections.IsGrounded && CanMove)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            wasRunningAtJump = IsRunning;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && CanMove)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    // Set the player's facing direction based on movement input
    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }
}
