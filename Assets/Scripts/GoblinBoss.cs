using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBoss : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float walkStopRate = 0.05f;
    public float dashSpeed = 10f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public Transform player;

    [SerializeField] private float directionChangeCooldown = 1.0f;
    private float directionChangeTimer = 0.0f;

    [Range(0f, 1f)]
    public float comboAttackProbability = 0.5f;

    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Animator animator;
    private Damageable damageable;
    private AudioSource audioSource;
    private AudioSource walkingAudioSource;

    public AudioClip[] goblinBossSounds;
    public AudioClip dashAttackSound;
    public AudioClip comboAttackSound;
    public AudioClip walkingSound;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;

    private bool isVisible = false;

    private bool hasPlayedComboSound = false;
    public AudioClip bossAppearanceMusic; // Add this field for the boss music

    private bool hasPlayedBossMusic = false; // Track if boss music has been played

    private const string ComboAttackAnimationName = "goblinboss_comboattack";

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set
        {
            if (_walkDirection != value)
            {
                // Only allow direction change if the timer has elapsed
                if (directionChangeTimer <= 0f)
                {
                    // Flip the sprite and update direction vector
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);

                    if (value == WalkableDirection.Right)
                    {
                        walkDirectionVector = Vector2.right;
                    }
                    else if (value == WalkableDirection.Left)
                    {
                        walkDirectionVector = Vector2.left;
                    }

                    // Reset the timer after changing direction
                    directionChangeTimer = directionChangeCooldown;
                }
            }

            _walkDirection = value;
        }
    }

    private bool _hasTarget = false;

    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public float AttackCooldown
    {
        get
        {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        }
        private set
        {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
        audioSource = GetComponent<AudioSource>();

        // Add a separate AudioSource for walking sound
        walkingAudioSource = gameObject.AddComponent<AudioSource>();
        walkingAudioSource.playOnAwake = false;
        walkingAudioSource.loop = true;
        walkingAudioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        // Check if the current animation is "goblinboss_comboattack"
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(ComboAttackAnimationName))
        {
            // Play the combo attack sound only once per animation
            if (!hasPlayedComboSound)
            {
                PlayComboAttackSound();
                hasPlayedComboSound = true;
            }
        }
        else
        {
            // Reset the flag when not in the combo attack animation
            hasPlayedComboSound = false;
        }

        // Other existing logic for updating target and cooldown timers
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }

        if (directionChangeTimer > 0)
        {
            directionChangeTimer -= Time.deltaTime;
        }
    }


    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall)
        {
            FlipDirection();
        }

        if (!damageable.LockVelocity)
        {
            if (CanMove)
            {
                FollowPlayer();
                PlayRandomSound();
                PlayWalkingSound();
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, walkStopRate), rb.velocity.y);
                StopWalkingSound();
            }
        }
    }

    public void DashMove()
    {
        // Move the enemy forward a bit
        Vector2 dashDirection = WalkDirection == WalkableDirection.Right ? Vector2.right : Vector2.left;
        rb.velocity = new Vector2(dashDirection.x * dashSpeed, rb.velocity.y);

        // Play dash attack sound if visible
        if (isVisible && dashAttackSound != null)
        {
            audioSource.PlayOneShot(dashAttackSound);
        }
    }

    public void PlayComboAttackSound()
    {
        if (isVisible && comboAttackSound != null)
        {
            audioSource.PlayOneShot(comboAttackSound);
        }
    }

    private void FollowPlayer()
    {
        // Check if we need to change direction based on player position
        if ((player.position.x > transform.position.x && WalkDirection == WalkableDirection.Left) ||
            (player.position.x < transform.position.x && WalkDirection == WalkableDirection.Right))
        {
            // Only change direction if the timer has elapsed
            if (directionChangeTimer <= 0f)
            {
                WalkDirection = player.position.x > transform.position.x ? WalkableDirection.Right : WalkableDirection.Left;
            }
        }

        rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.velocity.y);
    }

    private void FlipDirection()
    {
        // Flip direction immediately if touching a wall
        WalkDirection = (WalkDirection == WalkableDirection.Right) ? WalkableDirection.Left : WalkableDirection.Right;
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }

    private void PlayRandomSound()
    {
        if (isVisible && !audioSource.isPlaying && goblinBossSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, goblinBossSounds.Length);
            audioSource.clip = goblinBossSounds[randomIndex];
            audioSource.Play();
        }
    }

    private void PlayWalkingSound()
    {
        if (isVisible && walkingSound != null && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.clip = walkingSound;
            walkingAudioSource.Play();
        }
    }

    private void StopWalkingSound()
    {
        if (walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Stop();
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;

        // Play the boss appearance music if it hasn't been played yet
        if (!hasPlayedBossMusic && bossAppearanceMusic != null)
        {
            audioSource.clip = bossAppearanceMusic;
            audioSource.loop = false; // Ensure it plays only once
            audioSource.Play();
            hasPlayedBossMusic = true; // Set the flag so it only plays once
        }
    }

    void OnBecameInvisible()
    {
        isVisible = false;
        StopWalkingSound(); // Stop the walking sound when not visible
    }
}
