using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement; // Player Movement Script
    public Transform cam;
    public Transform gunTip; // Empty game object for the start of the line renderer
    public LayerMask wall;
    public LineRenderer lineRenderer;

    [Header("Grappling")]
    public float maxDistance;
    public float grappleDelayTime;
    private Vector3 grapplePoint;
    public float overshootYAxis;
    private bool grappling;

    [Header("CoolDown")]
    public float grapplingCD;
    private float grapplingCDTimer;

    [Header("Input")]
    public KeyCode grappleKey;


    /// <summary>
    /// Gets the player movement
    /// </summary>
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }


    /// <summary>
    /// Checks if the grapple key is pressed and starts the grapple,
    /// and reduces the grapple cooldown timer by the time since the last frame.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (grapplingCDTimer > 0) grapplingCDTimer -= Time.deltaTime;
    }


    private void LateUpdate()
    {
        // Set line renderer start position
        if (grappling) lineRenderer.SetPosition(0, gunTip.position);
    }

    /// <summary>
    /// Starts the grapple, setting the grapple point to the point in front of the camera,
    /// or the max distance if there is no hit.
    /// </summary>
    private void StartGrapple()
    {
        if (grapplingCDTimer > 0) return; // Return if cooldown is not ready

        // Allow line renderer to be drawn
        grappling = true;
        lineRenderer.enabled = true;

        // Freeze player movement
        playerMovement.freeze = true;

        // Perform raycast check to see if there is a wall at the front of the camera
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, wall))
        {
            // Grapple towards the wall
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            // Grapple towards the max distance
            grapplePoint = cam.position + cam.forward * maxDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.SetPosition(1, grapplePoint); // Set line render end position
    }

    private void ExecuteGrapple()
    {
        playerMovement.freeze = false; // Unfreeze player movement

        // Calculate a point lower then the actual grapple point to perform a smoother jump (helps to avoid a straight movement)
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;

        // Calculate the vertical offset between the player and the grapple point and apply an overshoot force to create a smoother arc
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;
        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc); // Move player to grapple point

        Invoke(nameof(StopGrapple), 1f); // Stop grapple after 1 second
    }

    /// <summary>
    /// Stops the grappling action
    /// </summary>
    public void StopGrapple()
    {
        playerMovement.freeze = false; // Unfreeze player movement

        grapplingCDTimer = grapplingCD; // Set cooldown timer

        // Disable line renderer
        grappling = false;
        lineRenderer.enabled = false;
    }
}
