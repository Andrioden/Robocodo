using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class StackingRobotsOverhangController : MonoBehaviour
{

    public Canvas containerCanvas;
    public Button robotsButtonPrefab;

    private void Update()
    {
        RotateToCamera();
    }

    public void Initialize(List<RobotController> robots)
    {
        transform.localPosition = Vector3.zero;
        RotateToCamera();

        foreach (RobotController robot in robots.OrderBy(r => r.name))
        {
            Button robotsButton = Instantiate(robotsButtonPrefab);
            robotsButton.GetComponent<Image>().color = robot.Settings_Color();
            robotsButton.onClick.RemoveAllListeners();
            var robotNotDirectRef = robot; // Cant have direct reference, lul
            robotsButton.onClick.AddListener(() => { ClickOverhangButton(robotNotDirectRef); });
            robotsButton.transform.SetParent(containerCanvas.transform, false);
        }

        gameObject.SetActive(true);
    }

    private void RotateToCamera()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
    }

    private void ClickOverhangButton(RobotController robot)
    {
        MouseManager.instance.ClickGameObject(robot.transform.root.gameObject);
    }

}