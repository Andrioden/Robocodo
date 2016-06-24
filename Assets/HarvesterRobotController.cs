using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;

public class HarvesterRobotController : NetworkBehaviour, IClickable
{
    private List<string> instructions = new List<string>();

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Click()
    {
        if (isLocalPlayer)
        {
            AddInstruction("WALK UP");
            AddInstruction("WALK UP");
            AddInstruction("WALK UP");
            AddInstruction("WALK UP");
            CmdStartRobot();
        }
    }

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("CmdStartRobot");
        foreach (string instruction in instructions)
        {
            if (instruction == "WALK UP")
            {
                transform.position += new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
                Thread.Sleep(1000);
            }
        }
    }

    [Command]
    private void CmdAddInstruction(string instruction)
    {
        instructions.Add("WALK UP");
    }

    private void AddInstruction(string instruction)
    {
        instructions.Add("WALK UP");
        if (isClient)
            CmdAddInstruction(instruction);
    }



}
