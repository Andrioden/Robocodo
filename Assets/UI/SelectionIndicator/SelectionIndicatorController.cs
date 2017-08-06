using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class SelectionIndicatorController : MonoBehaviour
{
    public GameObject quad;
    private Vector3 defaultScale;
    private float robotScaleFactor = 0.8f;
    private GameObject lastSelection = null;

    public GameObject selectedObjectPanel;
    public Text nameText;
    public Text summaryText;

    void Awake()
    {
        defaultScale = transform.localScale;
        selectedObjectPanel.SetActive(false);
    }

    void Update()
    {
        if (MouseManager.instance.CurrentlySelectedObject != null)
        {
            quad.SetActive(true);
            transform.position = MouseManager.instance.CurrentlySelectedObject.transform.position;
            SetScale();
        }
        else
        {
            quad.SetActive(false);
            //selectedObjectPanel.SetActive(false);
        }

        if (MouseManager.instance.LastClickedObject != null && MouseManager.instance.LastClickedObject.GetGameObject() != MouseManager.instance.CurrentlySelectedObject)
        {
            nameText.text = MouseManager.instance.LastClickedObject.GetName();
            summaryText.text = MouseManager.instance.LastClickedObject.GetSummary();
            selectedObjectPanel.SetActive(true);
        }
        else
            selectedObjectPanel.SetActive(false);
    }

    private void SetScale()
    {
        if (lastSelection == MouseManager.instance.CurrentlySelectedObject)
            return;
        else
            lastSelection = MouseManager.instance.CurrentlySelectedObject;

        bool isRobot = false;
        var robotComponent = MouseManager.instance.CurrentlySelectedObject.GetComponent<RobotController>();
        if (robotComponent != null)
            isRobot = true;

        var scaleFactor = isRobot ? robotScaleFactor : 1f;
        transform.localScale = new Vector3(defaultScale.x * scaleFactor, defaultScale.y * scaleFactor, defaultScale.z * scaleFactor);
    }
}