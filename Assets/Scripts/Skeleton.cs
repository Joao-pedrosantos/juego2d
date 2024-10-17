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
            if (Vector2.Distance(player.position, transform.position) < 10)
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
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;
        if (AttackCooldown > 0)
        {
        AttackCooldown -= Time.deltaTime;
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
                FollowPlayer();
            else
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, walkStopRate), rb.velocity.y);
        }
    }

    private void FollowPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            WalkDirection = WalkableDirection.Left;
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
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }
}
