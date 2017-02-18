using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackingRobotsOverhangNeededDetector : MonoBehaviour
{

    RobotController robot;

    // Use this for initialization
    private void Start()
    {
        robot = GetComponentInParent<RobotController>();

        if (robot == null)
            throw new Exception("Could not find RobotController in parent, have you changed the structure of the robot prefab?");
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter for " + robot.name);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit for " + robot.name);
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Staying for " + robot.name);
    }
}
