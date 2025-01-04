using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;

    [Header("Movement")]
    private float horizontalInput;
    private float verticalInput;
    Vector3 moveDirection;
    public float movementSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float swingSpeed;
    private bool readyToJump = true;
    [HideInInspector] public bool freeze;
    [HideInInspector] public bool activeGrapple;
    [HideInInspector] public bool swinging;
    private Vector3 velocityToSet;
    private bool enableMovement;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Freeze rotation to ensure player doesn't flip
    }

    private void Update()
    {
        // Ground Check using raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        MovementInput(); // Taking inputs

        //Ground Drag
        if (isGrounded && !activeGrapple)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        //Freeze Player
        if (freeze) rb.velocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        Move(); // Move the player
    }


    /// <summary>
    /// Gets horizontal and vertical input from the player.
    /// </summary>
    private void MovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && isGrounded && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // Applying cooldown to jump
        }
    }

    /// <summary>
    /// Moves the player based on input direction by applying a force to the Rigidbody.
    /// </summary>
    private void Move()
    {
        if (activeGrapple) return; // Return if user are grappling
        if(swinging) return;

        // Giving direction based on player input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Moving player (Applying force)
        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);
        }

    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); // Apply jump force
    }

    private void ResetJump()
    {
        readyToJump = true; // Reset jump
    }

    /// <summary>
    /// Initiates a jump towards the target position following a parabolic trajectory.
    /// </summary>
    /// <param name="targetPosition">The target position</param>
    /// <param name="trajectoryHeight">The height of the arc</param>
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
    }

    /// <summary>
    /// Sets the velocity of the rigidbody to the precalculated jump velocity.
    /// </summary>
    private void SetVelocity()
    {
        enableMovement = true;
        rb.velocity = velocityToSet;
    }

    // Apply force by using parabolic trajectory formula
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y; // Get gravity
        float displacementY = endPoint.y - startPoint.y; // Calculate vertical displacement between player and grapple point
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z); // Calculate horizontal displacement between player and grapple point

        // Use v^2 = u^2 + 2as (0 = u upward ^2 + (2 * gravity * height)) to calculate the upward force (u upward)
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);

        /* Use s = ut + (at^2) / 2 (displacement = u horizontal * (time up + time down) + (0 * (time up + time down)^2) / 2)
        *  to calculate the horizontal force (u horizontal)
        */
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Enable user to move again and stop grappling
        if (enableMovement)
        {
            enableMovement = false;
            activeGrapple = false;

            GetComponent<Grappling>().StopGrapple();
        }
    }
}
