using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 15f;
    public float jumpImpulse = 10f;
    public float airWalkSpeed = 5f;
    Vector2 moveInput;
    TouchingDirections touchingDirections;

    public AudioSource footstepAudio;  // Reference to the AudioSource for footstep sounds

    public float CurrentMoveSpeed 
    { 
        get 
        {
            if (IsMoving && !touchingDirections.IsOnWall)
            {
                if (touchingDirections.IsGrounded)
                {
                    return walkSpeed;
                }
                else
                {
                    return airWalkSpeed;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public bool _isMoving = false;

    public bool IsMoving 
    { 
        get 
        {
            return _isMoving;
        }
        private set 
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
            HandleFootsteps();  // Play or stop footstep sounds based on movement
        }
    }

    public bool _isFacingRight = true;
    public bool IsFacingRight 
    { 
        get 
        { 
            return _isFacingRight; 
        } 
        private set 
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput != Vector2.zero;
        SetFacingDirection(moveInput);
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
        }
    }

    // Play or stop footstep sound based on movement and grounded status
    private void HandleFootsteps()
    {
        if (IsMoving && touchingDirections.IsGrounded && !footstepAudio.isPlaying)
        {
            footstepAudio.Play();  // Play footstep sound
        }
        else if ((!IsMoving || !touchingDirections.IsGrounded) && footstepAudio.isPlaying)
        {
            footstepAudio.Stop();  // Stop footstep sound
        }
    }
}
