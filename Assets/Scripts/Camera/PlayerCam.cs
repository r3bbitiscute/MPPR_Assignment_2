using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float mouseSensX;
    public float mouseSensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock Cursor
    }

    /// <summary>
    /// Updates the camera rotation based on mouse input, constraining the vertical rotation
    /// to prevent the camera from flipping upside down and adjusts the player's orientation accordingly.
    /// </summary>
    private void Update()
    {
        // Mouse Input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensX;

        // Rotate Player
        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ensure that the camera cannot go upside down

        // Rotate Camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        //Rotate Orientation Object
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
