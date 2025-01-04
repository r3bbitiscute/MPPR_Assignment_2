using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform cameraPosition;

    /// <summary>
    /// Updates the camera's position to match the target camera position.
    /// </summary>
    private void Update()
    {
        transform.position = cameraPosition.position;
    }
}
