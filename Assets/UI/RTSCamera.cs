using UnityEngine;
using System.Collections;
using System;

/* Source and credit: C Sharp Accent Tutorials - Unity 3d Quick Tutorial RTS Camera with Automatic Height Adjustment https://youtu.be/QLOcykNgl7M */

public class RTSCamera : MonoBehaviour
{
    public float cameraHeight = 10;
    public float cameraDistance = 4;
    public float horizontalSpeed = 40;
    public float verticalSpeed = 40;
    public int edgeScrollBoundrary = 15;

    private int screenHeight, screenWidth;

    public static RTSCamera instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;
    }

    private void LateUpdate()
    {
        if (!KeyboardManager.KeyboardLock)
            MoveCameraWithKeyboard();

        if(Settings.GUI_EnableEdgeScrolling)
            MoveCameraWithMouseAndScreenEdge();
        ZoomCameraWithScrollWheel();
    }

    public void PositionRelativeTo(Transform relativeToTransform, float xOffset = 0.0f, float zOffSet = 0.0f)
    {
        transform.position = new Vector3(relativeToTransform.position.x, cameraHeight, relativeToTransform.position.z - cameraDistance);
    }

    private void MoveCameraWithKeyboard()
    {
        float horizontal = Input.GetAxis("Horizontal") * horizontalSpeed * Time.deltaTime / Time.timeScale;
        float vertical = Input.GetAxis("Vertical") * verticalSpeed * Time.deltaTime / Time.timeScale;

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);
    }

    private void MoveCameraWithMouseAndScreenEdge()
    {
        if (Input.mousePosition.x > (screenWidth - edgeScrollBoundrary) && Input.mousePosition.x < screenWidth)
            transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime);

        if (Input.mousePosition.x < (0 + edgeScrollBoundrary) && Input.mousePosition.x > 0)
            transform.Translate(Vector3.left * horizontalSpeed * Time.deltaTime);

        if (Input.mousePosition.y > (screenHeight - edgeScrollBoundrary) && Input.mousePosition.y < screenHeight)
            transform.Translate(Vector3.forward * verticalSpeed * Time.deltaTime);

        if (Input.mousePosition.y < (0 + edgeScrollBoundrary) && Input.mousePosition.y > 0)
            transform.Translate(Vector3.back * verticalSpeed * Time.deltaTime);
    }

    private void ZoomCameraWithScrollWheel()
    {
        var scollInput = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.down * scollInput * 15f);
    }
}
