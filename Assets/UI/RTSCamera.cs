using UnityEngine;
using System.Collections;
using System;

/* Source and credit: C Sharp Accent Tutorials - Unity 3d Quick Tutorial RTS Camera with Automatic Height Adjustment https://youtu.be/QLOcykNgl7M */

public class RTSCamera : MonoBehaviour
{
    public float horizontalSpeed = 40;
    public float verticalSpeed = 40;
    public float cameraDistance = 30;
    public int edgeScrollBoundrary = 5;

    private int screenHeight, screenWidth;

    void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;
    }

    void Update()
    {
        if (!KeyboardManager.KeyboardLock)
            MoveCameraWithKeyboard();

        MoveCameraWithMouseAndScreenEdge();
        ZoomCameraWithScrollWheel();
    }

    public void PositionRelativeToPlayer(Transform player)
    {
        transform.position = new Vector3(player.transform.position.x, cameraDistance, player.transform.position.z + 5f);
    }

    private void MoveCameraWithKeyboard()
    {
        float horizontal = Input.GetAxis("Horizontal") * horizontalSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * verticalSpeed * Time.deltaTime;

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);
    }

    private void MoveCameraWithMouseAndScreenEdge()
    {
        if (Input.mousePosition.x > (screenWidth - edgeScrollBoundrary))
            transform.Translate(Vector3.right * verticalSpeed * Time.deltaTime);

        if (Input.mousePosition.x < (0 + edgeScrollBoundrary))
            transform.Translate(Vector3.left * verticalSpeed * Time.deltaTime);

        if (Input.mousePosition.y > (screenHeight - edgeScrollBoundrary))
            transform.Translate(Vector3.forward * verticalSpeed * Time.deltaTime);

        if (Input.mousePosition.y < (0 + edgeScrollBoundrary))
            transform.Translate(Vector3.back * verticalSpeed * Time.deltaTime);
    }

    private void ZoomCameraWithScrollWheel()
    {
        var scollInput = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.down * scollInput * 15f);
    }
}
