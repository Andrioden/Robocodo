using UnityEngine;
using System.Collections;
using System;

public class SelectionIndicatorController : MonoBehaviour
{
    public GameObject quad;
    Vector3 defaultScale;
    float robotScaleFactor = 0.8f;
    GameObject lastSelection = null;

    void Awake()
    {
        defaultScale = transform.localScale;
    }

    void Update()
    {
        if (MouseManager.currentlySelected != null)
        {
            quad.SetActive(true);
            transform.position = MouseManager.currentlySelected.transform.position;
            SetScale();
        }
        else
            quad.SetActive(false);
    }

    private void SetScale()
    {
        if (lastSelection == MouseManager.currentlySelected)
            return;
        else
            lastSelection = MouseManager.currentlySelected;

        bool isRobot = false;
        var robotComponent = MouseManager.currentlySelected.GetComponent<RobotController>();
        if (robotComponent != null)
            isRobot = true;

        var scaleFactor = isRobot ? robotScaleFactor : 1f;
        transform.localScale = new Vector3(defaultScale.x * scaleFactor, defaultScale.y * scaleFactor, defaultScale.z * scaleFactor);
    }
}