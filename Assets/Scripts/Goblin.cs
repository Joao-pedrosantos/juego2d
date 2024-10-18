using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))] // Ensures AudioSource is attached
public class Goblin : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float walkStopRate = 0.05f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;

    public AudioClip sound1;  // First sound
    public AudioClip sound2;  // Second sound
    private AudioSource audioSource;

    public float minRandomSoundInterval = 1f; // Minimum interval to play sound
    public float maxRandomSoundInterval = 5f; // Maximum interval to play sound
    private float nextSoundTime = 0f; // Timer for next sound

    public Transform playerTransform;  // Reference to the player's transform
    public float soundDistanceThreshold = 5f;  // Distance within which the goblin will play sound

    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;
    Damageable damageable;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);

                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }

            _walkDirection = value;
            }
        }

    public bool _hasTarget = false;

    public bool HasTarget {
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

    public float AttackCooldown { get {
            return animator.GetFloat(AnimationStrings.attackCooldown);
    } private set {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
    }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();

        // Automatically grab the AudioSource component
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on the Goblin!");
        }

        SetNextSoundTime(); // Set the initial random time
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }

        // Check if it's time to play a sound and if the player is nearby
        if (Time.time >= nextSoundTime && IsPlayerNearby())
        {
            PlayRandomSound();
            SetNextSoundTime(); // Reset the timer for the next random sound
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
                rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.velocity.y);
            else
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, walkStopRate), rb.velocity.y);
        }
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.LogError("Current walk direction is not set to Left or Right");
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
        PlayRandomSound();  // Play a random sound when the goblin is hit
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
        if (audioSource != null && sound1 != null && sound2 != null)
        {
            AudioClip randomClip = (Random.Range(0, 2) == 0) ? sound1 : sound2;
            audioSource.PlayOneShot(randomClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClips are missing!");
        }
    }

    // Set a random time for the next sound to play
    private void SetNextSoundTime()
    {
        nextSoundTime = Time.time + Random.Range(minRandomSoundInterval, maxRandomSoundInterval);
    }

    // Check if the player is nearby within the sound distance threshold
    private bool IsPlayerNearby()
    {
        if (playerTransform == null)
        {
            return false;  // If the player's transform is not assigned, return false
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= soundDistanceThreshold;  // Check if the player is within range
    }
}
