using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class Skeleton : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float walkStopRate = 0.05f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public Transform player;

    [SerializeField] private float directionChangeCooldown = 1.0f; // Minimum time before changing direction
    private float directionChangeTimer = 0.0f; // Timer to track direction change

    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;
    Damageable damageable;
    AudioSource audioSource;

    public AudioClip[] skeletonSounds;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;

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
                    gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);

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

    public bool _hasTarget = false;

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
            if (Vector2.Distance(player.position, transform.position) < 10 && damageable.IsAlive)
            {
                animator.SetBool(AnimationStrings.canMove, true);
                return animator.GetBool(AnimationStrings.canMove);
            }
            else
            {
                animator.SetBool(AnimationStrings.canMove, false);
                return false;
            }
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
    }

    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        // Update the attack cooldown timer
        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }

        // Update the direction change timer
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
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, walkStopRate), rb.velocity.y);
            }
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
        if (!audioSource.isPlaying && skeletonSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, skeletonSounds.Length);
            audioSource.clip = skeletonSounds[randomIndex];
            audioSource.Play();
        }
    }
}
