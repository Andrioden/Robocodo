using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class GarageRobot : MonoBehaviour
{
    public RobotController robot;
    public Button button;
    public Image icon;
    public Text nameLabel;
    public Text statusLabel;

    public void SetupGarageRobot(RobotController robot)
    {
        this.robot = robot;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectRobot);
        icon.sprite = robot.Sprite();
        nameLabel.text = robot.Settings_Name().ToUpper();
    }

    private void Update()
    {
        if (!robot)
            return;

        statusLabel.text = robot.Feedback;
    }

    private void SelectRobot()
    {
        MouseManager.SimulateClick(robot.transform.root.gameObject);
    }
}
