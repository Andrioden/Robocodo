using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class SelectionIndicatorController : MonoBehaviour
{
    public GameObject quad;
    private Vector3 defaultScale;
    private float robotScaleFactor = 0.8f;
    private GameObject selectedGameObjectlastFrame = null;
    private IClickable lastClickedObjectCache;

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
            quad.SetActive(false);

        var lastClickedGO = MouseManager.instance.LastClickedObject;
        if (lastClickedGO != null && lastClickedGO != MouseManager.instance.CurrentlySelectedObject)
        {
            if (lastClickedObjectCache == null || lastClickedObjectCache.GetGameObject() != lastClickedGO)
                lastClickedObjectCache = lastClickedGO.GetComponent<IClickable>();

            nameText.text = lastClickedObjectCache.GetName();
            summaryText.text = lastClickedObjectCache.GetSummary();
            selectedObjectPanel.SetActive(true);
        }
        else
        {
            selectedObjectPanel.SetActive(false);
            lastClickedObjectCache = null;
        }
    }

    private void SetScale()
    {
        if (selectedGameObjectlastFrame == MouseManager.instance.CurrentlySelectedObject)
            return;
        else
            selectedGameObjectlastFrame = MouseManager.instance.CurrentlySelectedObject;

        bool isRobot = false;
        var robotComponent = MouseManager.instance.CurrentlySelectedObject.GetComponent<RobotController>();
        if (robotComponent != null)
            isRobot = true;

        var scaleFactor = isRobot ? robotScaleFactor : 1f;
        transform.localScale = new Vector3(defaultScale.x * scaleFactor, defaultScale.y * scaleFactor, defaultScale.z * scaleFactor);
    }
}