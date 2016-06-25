using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

public class HarvesterRobotController : NetworkBehaviour, IClickable
{
    public MeshRenderer bodyMeshRenderer;

    [SyncVar]
    float posX;

    [SyncVar]
    float posZ;

    private List<string> instructions = new List<string>();

    private int speed = 2;
    private bool executingInstructions = false;

    private void Start()
    {
        posX = transform.position.x;
        posZ = transform.position.z;
    }

    // Update is called once per frame
    private void Update()
    {
        var newPos = new Vector3(posX, transform.position.y, posZ);
        transform.LookAt(newPos);
        transform.position = Vector3.MoveTowards(transform.position, newPos, 1.05f * Time.deltaTime);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        bodyMeshRenderer.material.color = Color.blue;
    }

    public void Click()
    {
        if (hasAuthority)
        {
            CmdClearInstruction();
            CmdAddInstruction("WALK UP");
            CmdAddInstruction("WALK UP");
            CmdAddInstruction("WALK UP");
            CmdAddInstruction("WALK UP");
            CmdAddInstruction("WALK RIGHT");
            CmdAddInstruction("WALK RIGHT");
            CmdAddInstruction("WALK DOWN");
            CmdAddInstruction("WALK LEFT");
            CmdAddInstruction("WALK LEFT");
            CmdAddInstruction("WALK UP");
            CmdStartRobot();
        }
    }

    [Command]
    private void CmdStartRobot()
    {
        if (!executingInstructions)
            StartCoroutine(ExecuteInstructionsCoroutine());
        else
            Debug.Log("Already executing instructions!!");
    }

    IEnumerator ExecuteInstructionsCoroutine()
    {
        executingInstructions = true;

        foreach (string instruction in instructions)
        {
            Debug.Log(instruction);
            if (instruction == "WALK UP")
                posZ++;
            else if (instruction == "WALK DOWN")
                posZ--;
            else if (instruction == "WALK RIGHT")
                posX++;
            else if (instruction == "WALK LEFT")
                posX--;

            yield return new WaitForSeconds(1f);
        }

        executingInstructions = false;
    }

    [Command]
    private void CmdAddInstruction(string instruction)
    {
        instructions.Add(instruction);
    }

    [Command]
    private void CmdClearInstruction()
    {
        instructions = new List<string>();
    }
}
