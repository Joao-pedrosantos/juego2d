using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    // Movement-related fields
    public float runSpeed = 8f;
    public float jumpImpulse = 10.5f;
    public float airRunSpeed = 6f;
    public float maxVerticalSpeed = 10f;
    private Vector2 moveInput;

    private bool isDead = false;

    // Audio-related fields
    public AudioSource footstepAudio;
    public AudioSource attackAudio;     
    public AudioSource jumpAudio;
    public AudioClip attackClip;        
    public AudioClip jumpClip;
    public AudioClip runningStoneClip;  
    public AudioSource deathAudio;      
    public AudioClip deathClip;         

    public float runPitch = 1.3f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;
    private Damageable damageable;

    // Player state flags
    private bool _isMoving;
    private bool _isFacingRight = true;

    // Game over flags
    public GameObject GameOverPanel;
    public float gameOverHeight = -15f; 

    // You Win flags
    public GameObject VictoryPanel;
    public float victoryThreshold = 0;

    public GameObject HealthBar;

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
        ? (touchingDirections.IsGrounded ? runSpeed : airRunSpeed)
        : 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
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

        if (transform.position.y <= gameOverHeight)
        {
            animator.SetBool(AnimationStrings.isAlive, false);
        }

        if (SceneManager.GetActiveScene().name == "2GameScene" || SceneManager.GetActiveScene().name == "GameScene")
        {
            CheckVictoryConditions();
        }
        if (SceneManager.GetActiveScene().name == "3GameScene")
        {
            CheckVictoryConditionsMap3();
        }
    }

    // Handles the main movement logic
    private void HandleMovement()
    {
        if (touchingDirections.IsGrounded && touchingDirections.slopeAngle <= 60)
        {
            Vector2 adjustedMovement = CalculateSlopeMovement();
            rb.velocity = new Vector2(adjustedMovement.x, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput.x * runSpeed, Mathf.Clamp(rb.velocity.y, -maxVerticalSpeed, maxVerticalSpeed));
        }
    }

    private Vector2 CalculateSlopeMovement()
    {
        Vector2 slopeNormal = touchingDirections.slopeNormal.normalized;
        Vector2 slopeParallel = new Vector2(slopeNormal.y, -slopeNormal.x);
        return slopeParallel * moveInput.x * runSpeed;
    }

    private void HandleFootsteps()
    {
        if (IsMoving && touchingDirections.IsGrounded)
        {
            if (!footstepAudio.isPlaying)
            {
                footstepAudio.Play();
            }
            footstepAudio.pitch = runPitch;
        }
        else if (footstepAudio.isPlaying)
        {
            footstepAudio.Stop();
        }
    }

    // Touch-based input handlers for movement
    public void OnMoveLeft(bool isPressed)
    {
        if (isPressed)
        {
            moveInput = Vector2.left;
            IsMoving = true;
            SetFacingDirection(moveInput);
        }
        else
        {
            moveInput = Vector2.zero;
            IsMoving = false;
        }
    }

    public void OnMoveRight(bool isPressed)
    {
        if (isPressed)
        {
            moveInput = Vector2.right;
            IsMoving = true;
            SetFacingDirection(moveInput);
        }
        else
        {
            moveInput = Vector2.zero;
            IsMoving = false;
        }
    }


    // Touch-based input handlers for jumping
    public void OnJumpButtonPressed()
    {
        if (touchingDirections.IsGrounded && CanMove)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

            if (jumpAudio != null && jumpClip != null)
            {
                jumpAudio.PlayOneShot(jumpClip);
            }
        }
    }

    // Touch-based input handlers for attacking
    public void OnAttackButtonPressed()
    {
        if (CanMove && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);

            if (attackAudio != null && attackClip != null)
            {
                attackAudio.PlayOneShot(attackClip);  // Play attack sound without interrupting other sounds
            }
        }
    }

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

    public void GameOver()
    {
        animator.SetBool(AnimationStrings.isAlive, false);
        GameOverPanel.SetActive(true);

        if (deathAudio != null && deathClip != null && !isDead)
        {
            deathAudio.PlayOneShot(deathClip);
            isDead = true;
        }
    }

    public void CheckVictoryConditions()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Skeleton");
        if (enemies.Length == 0 && transform.position.x > victoryThreshold)
        {
            Victory();
        }
    }

    public void CheckVictoryConditionsMap3()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Boss");
        if (enemies.Length == 0)
        {
            HealthBar.SetActive(false);
            Victory();
        }
    }

    public void Victory()
    {
        VictoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
