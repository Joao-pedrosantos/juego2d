using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    public float wallDistance = 0.2f;
    public float ceilingDistance = 0.05f;

    public bool WasGroundedLastFrame { get; private set; }

    CapsuleCollider2D touchingCol;
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];

    [SerializeField]
    private bool _isGrounded;
    public bool IsGrounded 
    { 
        get 
        {
            return _isGrounded; 
        }
        private set 
        {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }
    }

    [SerializeField]
    private bool _isOnWall;
    public bool IsOnWall 
    { 
        get 
        {
            return _isOnWall; 
        }
        private set 
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
        }
    }

    [SerializeField]
    private bool _isOnCeiling;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    public bool IsOnCeiling 
    { 
        get 
        {
            return _isOnCeiling; 
        }
        private set 
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.isOnCeiling, value);
        }
    }

    public float slopeAngle;  // To store the angle of the slope
    public Vector2 slopeNormal; // To store the normal of the slope

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // Store the current grounded state before updating it
        WasGroundedLastFrame = IsGrounded;

        // Ground detection with slope angle
        int groundHitCount = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance);
        if (groundHitCount > 0)
        {
            IsGrounded = true;
            slopeNormal = groundHits[0].normal;  // Get the slope normal
            slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);  // Calculate slope angle
        }
        else
        {
            IsGrounded = false;
            slopeNormal = Vector2.up;
            slopeAngle = 0;
        }

        // Wall detection with angle check
        int wallHitCount = touchingCol.Cast(wallCheckDirection, castFilter, wallHits, wallDistance);
        if (wallHitCount > 0)
        {
            float wallHitAngle = Vector2.Angle(wallHits[0].normal, Vector2.up);
            IsOnWall = wallHitAngle > 75f;  // Only consider it a wall if the angle is steep (e.g., > 75 degrees)
        }
        else
        {
            IsOnWall = false;
        }

        // Ceiling detection
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
    }
}
