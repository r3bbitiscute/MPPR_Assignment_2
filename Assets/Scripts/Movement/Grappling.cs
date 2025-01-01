using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    public Transform cam;
    public Transform gunTip;
    public LayerMask wall;
    public LineRenderer lineRenderer;

    [Header("Grappling")]
    public float maxDistance;
    public float grappleDelayTime;
    private Vector3 grapplePoint;
    public float overshootYAxis;

    [Header("CoolDown")]
    public float grapplingCD;
    private float grapplingCDTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse0;
    private bool grappling;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (grapplingCDTimer > 0) grapplingCDTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            lineRenderer.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        if (grapplingCDTimer > 0) return;
        grappling = true;
        playerMovement.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, wall))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        playerMovement.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        playerMovement.freeze = false;
        grappling = false;
        grapplingCDTimer = grapplingCD;

        lineRenderer.enabled = false;
    }
}
