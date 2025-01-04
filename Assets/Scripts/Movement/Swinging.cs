using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public Transform gunTip,cam,player; // Empty game object for the start of the line renderer
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;
    private PlayerMovement pm;  // Player Movement Script

    [Header("Swinging")]
    public float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

   

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse1;


    /// <summary>
    /// Gets the player movement
    /// </summary>
    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// Checks if the grapple key is pressed and starts the grapple,
    /// and reduces the grapple cooldown timer by the time since the last frame.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

    }


    private void LateUpdate()
    {
        if(lr.enabled)DrawRope();
    }

    /// <summary>
    /// Starts the Swinging from the gun tip to the swing point using the camera to detect
    /// </summary>
    private void StartSwing()
    {
        pm.swinging = true;
        
        RaycastHit hit;
        if (Physics.Raycast(cam.position,cam.forward,out hit,maxSwingDistance, whatIsGrappleable))
        {
            swingPoint = hit.point;
            lr.enabled = true;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;
            
            float distanceFromPoint = Vector3.Distance(player.position,swingPoint);

            //The distance grapple will try to keep from grapple point
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
        
    }

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    /// <summary>
    /// Stops the swinging action
    /// </summary>
    public void StopSwing()
    {
       pm.swinging = false;
       
       lr.positionCount = 0;
       lr.enabled = false;
       Destroy(joint);
       
    }
}
