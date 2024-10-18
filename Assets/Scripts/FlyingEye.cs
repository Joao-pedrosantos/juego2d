using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEye : MonoBehaviour
{
    public float flightSpeed = 5f;
    public float waypointReachedDistance = 0.1f;
    public DetectionZone biteDetectionZone;
    public Collider2D deathCollider;
    public List<Transform> waypoints;
    public AudioSource flightAudioSource; // Audio source for the movement sound
    public AudioClip flightClip; // Flight sound clip
    public float fadeDuration = 1f; // Duration for fading in and out
    public int maxHP = 100; // Enemy's max health
    private int currentHP; // Enemy's current health

    Animator animator;
    Rigidbody2D rb;
    Damageable damageable;

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

        // Initialize HP
        currentHP = maxHP;

        // Set up the AudioSource component
        flightAudioSource = GetComponent<AudioSource>();
        if (flightAudioSource != null && flightClip != null)
        {
            flightAudioSource.clip = flightClip;
            flightAudioSource.loop = true; // Loop the sound during flight
            flightAudioSource.volume = 0f; // Start with volume 0 for fade-in
            flightAudioSource.Play(); // Ensure the sound is playing
        }
    }

    private void Start()
    {
        nextWaypoint = waypoints[waypointNum];
    }

    private void OnEnable()
    {
        damageable.damageableDeath.AddListener(OnDeath);
    }

    void Update()
    {
        HasTarget = biteDetectionZone.detectedColliders.Count > 0;
    }

    private void FixedUpdate()
    {
        if (damageable.IsAlive && currentHP > 0)
        {
            if (CanMove)
            {
                Flight();
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

        if (distance > 15f)
        {
            waypointNum++;
            if (waypointNum >= waypoints.Count)
            {
                waypointNum = 0;
            }

            nextWaypoint = waypoints[waypointNum];
            distance = Vector2.Distance(nextWaypoint.position, transform.position);
        }

        directionToWaypoint = (nextWaypoint.position - transform.position).normalized;
        rb.velocity = directionToWaypoint * flightSpeed;

        UpdateDirection();

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

    // Method to take damage and reduce HP
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        // Check if NPC is dead
        if (currentHP <= 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        rb.gravityScale = 1f;
        rb.velocity = new Vector2(0, rb.velocity.y);
        deathCollider.enabled = true;

        // Instead of stopping the sound immediately, fade it out
        StartCoroutine(FadeOutAndStopSound());
    }

    // Triggered when the NPC enters the camera's view
    private void OnBecameVisible()
    {
        StartCoroutine(FadeInSound());
    }

    // Triggered when the NPC leaves the camera's view
    private void OnBecameInvisible()
    {
        StartCoroutine(FadeOutSound());
    }

    // Coroutine to fade in the sound
    private IEnumerator FadeInSound()
    {
        float startVolume = flightAudioSource.volume;
        float time = 0;

        // Ensure sound is playing
        if (!flightAudioSource.isPlaying)
        {
            flightAudioSource.Play();
        }

        // Gradually increase the volume
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            flightAudioSource.volume = Mathf.Lerp(startVolume, 1f, time / fadeDuration);
            yield return null;
        }

        flightAudioSource.volume = 1f; // Ensure it's fully at max volume
    }

    // Coroutine to fade out the sound
    private IEnumerator FadeOutSound()
    {
        float startVolume = flightAudioSource.volume;
        float time = 0;

        // Gradually decrease the volume
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            flightAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        flightAudioSource.volume = 0f; // Ensure it's fully muted
    }

    // Coroutine to fade out and stop the sound on death
    private IEnumerator FadeOutAndStopSound()
    {
        float startVolume = flightAudioSource.volume;
        float time = 0;

        // Gradually decrease the volume
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            flightAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        flightAudioSource.volume = 0f; // Ensure it's fully muted
        flightAudioSource.Stop(); // Stop playing after fade-out
    }
}
