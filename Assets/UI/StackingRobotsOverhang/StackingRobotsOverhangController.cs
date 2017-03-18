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
        transform.position = new Vector3(robots[0].x, transform.position.y, robots[0].z);
        RotateToCamera();

        foreach (RobotController robot in robots.OrderBy(r => r.name))
        {
            Button robotsButton = Instantiate(robotsButtonPrefab);
            robotsButton.GetComponent<Image>().color = robot.Settings_Color();
            robotsButton.onClick.RemoveAllListeners();
            var robotTet = robot;
            robotsButton.onClick.AddListener(() => { ClickRobot(robotTet); });
            robotsButton.transform.SetParent(containerCanvas.transform, false);
        }

        gameObject.SetActive(true);
    }

    private void RotateToCamera()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
    }


    private void ClickRobot(RobotController robot)
    {
        MouseManager.ClickAndSelectGameObject(robot.transform.root.gameObject);
    }

}