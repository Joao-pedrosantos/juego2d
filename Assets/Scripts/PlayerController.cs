using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    // Movement-related fields
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpImpulse = 8f;
    public float airWalkSpeed = 3f;
    public float airRunSpeed = 6f;
    public float maxVerticalSpeed = 10f;
    private bool wasRunningAtJump;
    private bool runInputWhileAirborne;
    private Vector2 moveInput;

    private bool isDead = false;

    private float idleTime = 0f;
    private float idleThreshold = 0.3f;
    private float idleAcceleration = -2f;

    // Audio-related fields
    public AudioSource footstepAudio;   // For footstep sounds
    public AudioSource attackAudio;     // Separate AudioSource for attack sound
    public AudioSource jumpAudio;       // Separate AudioSource for jump sound
    public AudioClip attackClip;        // The attack sound clip
    public AudioClip jumpClip;          // The jump sound clip

    public AudioClip walkingStoneClip; // Footstep sound for walking on stone
    public AudioClip runningStoneClip; // Footstep sound for running on stone

    public AudioSource hitAudio;        // Separate AudioSource for hit sound
    public AudioClip[] hitClips;        // Array of hit sound clips

    public AudioSource deathAudio;      // Separate AudioSource for death sound
    public AudioClip deathClip;         // The death sound clip

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

    // Game over flags
    public GameObject GameOverPanel;
    public float gameOverHeight = -15f; // Defina a altura para game over

    // You Win flags
    public GameObject VictoryPanel;
    public float victoryThreshold = 0; // Defina a posição para vitória

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
        ? (touchingDirections.IsGrounded ? (IsRunning ? runSpeed : walkSpeed) : (wasRunningAtJump ? airRunSpeed : airWalkSpeed))
        : 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!IsAlive)
        {
            GameOver();
        }
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

        WindEffect(); // Se quiser tirar é so comentar essa funcao

        if (transform.position.y <= gameOverHeight)
        {
            animator.SetBool(AnimationStrings.isAlive, false);
        }
        if (SceneManager.GetActiveScene().name == "2GameScene" || SceneManager.GetActiveScene().name == "GameScene"){
            CheckVictoryConditions();
        }
        if (SceneManager.GetActiveScene().name == "3GameScene")
        {
            CheckVictoryConditionsMap3();
        }
        
    }

    private void WindEffect()
    {
        if (SceneManager.GetActiveScene().name == "2GameScene")
        {
            if (!IsMoving && touchingDirections.IsGrounded)
            {
                idleTime += Time.deltaTime;
                if (idleTime >= idleThreshold)
                {
                    rb.velocity = new Vector2(idleAcceleration, rb.velocity.y);
                }
            }
            else
            {
                idleTime = 0f;
            }
        }
    }

    // Handles the main movement logic
    private void HandleMovement()
    {
        if (touchingDirections.IsGrounded && touchingDirections.slopeAngle <= 60)
        {
            Vector2 adjustedMovement = CalculateSlopeMovement();
            float targetYVelocity = AdjustVerticalMovementForSlope();

            SmoothSnapToGround(ref targetYVelocity);

            targetYVelocity = Mathf.Clamp(targetYVelocity, -maxVerticalSpeed, maxVerticalSpeed);

            rb.velocity = new Vector2(adjustedMovement.x, targetYVelocity);
        }
        else
        {
            rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, Mathf.Clamp(rb.velocity.y, -maxVerticalSpeed, maxVerticalSpeed));
        }
    }

    private Vector2 CalculateSlopeMovement()
    {
        Vector2 slopeNormal = touchingDirections.slopeNormal.normalized;
        Vector2 slopeParallel = new Vector2(slopeNormal.y, -slopeNormal.x);
        return slopeParallel * moveInput.x * CurrentMoveSpeed;
    }

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

    private void SmoothSnapToGround(ref float targetYVelocity)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, touchingDirections.castFilter.layerMask);
        if (hit.collider != null && rb.velocity.y <= 0)
        {
            float distanceToGround = hit.distance;

            if (distanceToGround > 0.01f && distanceToGround < 0.5f)
            {
                float snapSpeed = Mathf.Clamp(distanceToGround * 10f, 0f, 10f);
                rb.velocity = new Vector2(rb.velocity.x, -snapSpeed);
                targetYVelocity = Mathf.Lerp(targetYVelocity, 0, 0.1f);
            }
            else if (distanceToGround >= 0.5f)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - distanceToGround, transform.position.z);
                targetYVelocity = 0;
            }
        }
    }

    private void HandleLanding()
    {
        if (!touchingDirections.WasGroundedLastFrame && touchingDirections.IsGrounded)
        {
            IsRunning = runInputWhileAirborne;
        }
    }

    private void HandleFootsteps()
    {
        // Check if the current scene is 3GameScene
        if (SceneManager.GetActiveScene().name == "3GameScene")
        {
            // Play sound only if the character is moving and grounded
            if (IsMoving && touchingDirections.IsGrounded)
            {
                // Set the correct audio clip based on walking or running
                footstepAudio.clip = IsRunning ? runningStoneClip : walkingStoneClip;
                footstepAudio.pitch = IsRunning ? runPitch : walkPitch;

                // Ensure the correct clip is playing
                if (!footstepAudio.isPlaying || footstepAudio.clip != (IsRunning ? runningStoneClip : walkingStoneClip))
                {
                    footstepAudio.Play();
                }
            }
            else if (footstepAudio.isPlaying) // Stop sound when not moving or airborne
            {
                footstepAudio.Stop();
            }
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

            // Play jump sound
            if (jumpAudio != null && jumpClip != null)
            {
                jumpAudio.PlayOneShot(jumpClip);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && CanMove && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);

            // Play attack sound
            if (attackAudio != null && attackClip != null)
            {
                attackAudio.PlayOneShot(attackClip);  // Play attack sound without interrupting other sounds
            }
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        // Apply knockback to the player
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);

        // Play a random hit sound
        if (hitAudio != null && hitClips != null && hitClips.Length > 0)
        {
            int index = Random.Range(0, hitClips.Length);
            AudioClip clipToPlay = hitClips[index];
            hitAudio.PlayOneShot(clipToPlay);
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
	if (CanMove)
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

    public void Respawn()
    {
        GameOverPanel.SetActive(false);
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        isDead = false;
    }

    public void GameOver()
    {
        animator.SetBool(AnimationStrings.isAlive, false);
        GameOverPanel.SetActive(true);

        // Play death sound
        if (deathAudio != null && deathClip != null && !isDead)
        {
            deathAudio.PlayOneShot(deathClip);
            isDead = true;
            
        }
    }

    public void CheckVictoryConditions()
    {
        // Verifica se todos os inimigos foram destruídos
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Skeleton");
        if (enemies.Length == 0 && transform.position.x > victoryThreshold)
        {
            Victory();
        }
    }

    public void CheckVictoryConditionsMap3()
    {
        // Verifica se todos os inimigos foram destruídos
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Boss");
        if (enemies.Length == 0 && transform.position.x > victoryThreshold)
        {
            Victory();
        }
    }

    public void Victory()
    {
        VictoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
