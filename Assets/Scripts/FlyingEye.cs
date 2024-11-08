using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Renderer))]
public class FlyingEye : MonoBehaviour
{
    public float flightSpeed = 5f;
    public float waypointReachedDistance = 0.1f;
    public DetectionZone biteDetectionZone;
    public Collider2D deathCollider;
    public List<Transform> waypoints;

    Animator animator;
    Rigidbody2D rb;
    Damageable damageable;
    AudioSource audioSource;  // AudioSource for playing sounds
    Renderer flyingEyeRenderer;  // Renderer for visibility checking

    public AudioClip[] flyingEyeSounds;  // Array of sound clips

    Transform nextWaypoint;
    int waypointNum = 0;

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
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
        audioSource = GetComponent<AudioSource>();  // Initialize the AudioSource
        flyingEyeRenderer = GetComponent<Renderer>();  // Initialize the Renderer
    }

    private void Start()
    {
        nextWaypoint = waypoints[waypointNum];
    }

    private void OnEnable()
    {
        damageable.damageableDeath.AddListener(OnDeath);
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = biteDetectionZone.detectedColliders.Count > 0;
    }

    private void FixedUpdate()
    {
        if (damageable.IsAlive)
        {
            if (CanMove)
            {
                Flight();
                PlayRandomSound();  // Play sound when moving and visible
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
        }
    }

    private void Flight()
    {
        Vector2 directionToWaypoint = (nextWaypoint.position - transform.position).normalized;

        float distance = Vector2.Distance(nextWaypoint.position, transform.position);

        // Skip waypoint if it's too far away
        if (distance > 15f)
        {
            waypointNum++;
            if (waypointNum >= waypoints.Count)
            {
                waypointNum = 0;
            }

            nextWaypoint = waypoints[waypointNum]; // Update to the new waypoint
            distance = Vector2.Distance(nextWaypoint.position, transform.position); // Recalculate distance
        }

        // Move towards the next waypoint
        directionToWaypoint = (nextWaypoint.position - transform.position).normalized;
        rb.velocity = directionToWaypoint * flightSpeed;

        UpdateDirection();

        // Check if we reached the waypoint or if it attacked the player (HasTarget is true)
        if (distance <= waypointReachedDistance || HasTarget)
        {
            waypointNum++;
            if (waypointNum >= waypoints.Count) 
            {
                waypointNum = 0;
            }

            nextWaypoint = waypoints[waypointNum];
        }
    }

    private void UpdateDirection()
    {
        Vector3 locScale = transform.localScale;

        if (transform.localScale.x > 0)
        {
            if (rb.velocity.x < 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
        else
        {
            if (rb.velocity.x > 0)
            {
                transform.localScale = new Vector3(-1 * locScale.x, locScale.y, locScale.z);
            }
        }
    }

    public void OnDeath()
    {
        rb.gravityScale = 1f;
        rb.velocity = new Vector2(0, rb.velocity.y);
        deathCollider.enabled = true;
    }

    private void PlayRandomSound()
    {
        // Check if the object is visible and AudioSource is not currently playing
        if (flyingEyeRenderer.isVisible && !audioSource.isPlaying && flyingEyeSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, flyingEyeSounds.Length);  // Choose a random sound
            audioSource.clip = flyingEyeSounds[randomIndex];  // Set the clip to play
            audioSource.Play();  // Play the selected sound
        }
    }
}
