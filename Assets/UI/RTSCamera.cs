using UnityEngine;
using System.Collections;

/* Source and credit: C Sharp Accent Tutorials - Unity 3d Quick Tutorial RTS Camera with Automatic Height Adjustment https://youtu.be/QLOcykNgl7M */

public class RTSCamera : MonoBehaviour
{
    public float horizontalSpeed = 40;
    public float verticalSpeed = 40;
    public float cameraDistance = 30;

    void Update()
    {
        if (!KeyboardManager.KeyboardLock)
            MoveCameraVertifalHorizontalDetector();

        MoveCameraUpDownDetector();
    }

    public void PositionRelativeToPlayer(Transform player)
    {
        transform.position = new Vector3(player.transform.position.x, cameraDistance, player.transform.position.z + 5f);
    }

    private void MoveCameraVertifalHorizontalDetector()
    {
        float horizontal = Input.GetAxis("Horizontal") * horizontalSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * verticalSpeed * Time.deltaTime;

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);
    }

    private void MoveCameraUpDownDetector()
    {
        var scollInput = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.down * scollInput * 15f);
    }
}
