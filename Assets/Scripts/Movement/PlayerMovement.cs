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
    private bool readyToJump = true;
    [HideInInspector] public bool freeze;
    [HideInInspector] public bool activeGrapple;
    private Vector3 velocityToSet;
    private bool enableMovementOnNextTouch;

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
        readyToJump = true;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
    }

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    public void ResetRestriction()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestriction();

            GetComponent<Grappling>().StopGrapple();
        }
    }
}
