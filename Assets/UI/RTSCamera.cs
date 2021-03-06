﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

/* Source and credit: C Sharp Accent Tutorials - Unity 3d Quick Tutorial RTS Camera with Automatic Height Adjustment https://youtu.be/QLOcykNgl7M */

public class RTSCamera : MonoBehaviour
{
    public float cameraStartHeight;
    public float cameraDistance;
    public float minScrollSpeed;
    public float maxScrollSpeed;
    public int edgeScrollBoundrary;
    public float minZoomDistance;
    public float maxZoomDistance;

    private int screenHeight;
    private int screenWidth;

    private float _scrollSpeed;

    public static RTSCamera instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;
        transform.position = new Vector3(transform.position.x, cameraStartHeight, transform.position.z);
        CacheScrollSpeedFromZoom();
    }

    private void LateUpdate()
    {
        if (!KeyboardManager.KeyboardLock)
            MoveCameraWithKeyboard();

        if (Settings.GUI_EnableEdgeScrolling)
            MoveCameraWithMouseAndScreenEdge();

        ZoomCameraWithScrollWheel();
    }

    public void PositionRelativeTo(Transform relativeToTransform, float xOffset = 0.0f, float zOffSet = 0.0f)
    {
        transform.position = new Vector3(relativeToTransform.position.x + xOffset, transform.position.y, relativeToTransform.position.z - cameraDistance + zOffSet);
    }

    private void MoveCameraWithKeyboard()
    {
        float horizontal = Input.GetAxis("Horizontal") * _scrollSpeed * Time.deltaTime / Math.Max(1, Time.timeScale);
        float vertical = Input.GetAxis("Vertical") * _scrollSpeed * Time.deltaTime / Math.Max(1, Time.timeScale);

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);
    }

    private void MoveCameraWithMouseAndScreenEdge()
    {
        if (Input.mousePosition.x > (screenWidth - edgeScrollBoundrary) && Input.mousePosition.x < screenWidth)
            transform.Translate(Vector3.right * minScrollSpeed * Time.deltaTime);

        if (Input.mousePosition.x < (0 + edgeScrollBoundrary) && Input.mousePosition.x > 0)
            transform.Translate(Vector3.left * minScrollSpeed * Time.deltaTime);

        if (Input.mousePosition.y > (screenHeight - edgeScrollBoundrary) && Input.mousePosition.y < screenHeight)
            transform.Translate(Vector3.forward * _scrollSpeed * Time.deltaTime);

        if (Input.mousePosition.y < (0 + edgeScrollBoundrary) && Input.mousePosition.y > 0)
            transform.Translate(Vector3.back * _scrollSpeed * Time.deltaTime);
    }

    private void ZoomCameraWithScrollWheel()
    {
        //Don't scroll if outside screen.
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (!screenRect.Contains(Input.mousePosition))
            return;

        //Don't scroll if over UI element.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        var scollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scollInput == 0)
            return;

        transform.Translate(Vector3.down * scollInput * 15f);

        //Restrict zoom levels
        if (transform.localPosition.y < minZoomDistance)
            transform.localPosition = new Vector3(transform.localPosition.x, minZoomDistance, transform.localPosition.z);
        else if (transform.localPosition.y > maxZoomDistance)
            transform.localPosition = new Vector3(transform.localPosition.x, maxZoomDistance, transform.localPosition.z);

        CacheScrollSpeedFromZoom();
    }

    private void CacheScrollSpeedFromZoom()
    {
        _scrollSpeed = (float)MathUtils.LinearConversionDouble(minZoomDistance, maxZoomDistance, minScrollSpeed, maxScrollSpeed, transform.position.y);
    }

}