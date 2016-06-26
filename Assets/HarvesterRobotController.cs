using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

public class HarvesterRobotController : NetworkBehaviour, IClickable
{
    public MeshRenderer visirMeshRenderer;

    [SyncVar]
    float posX;

    [SyncVar]
    float posZ;

    private List<string> instructions = new List<string>();
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
        visirMeshRenderer.material.color = Color.blue;
    }

    public void Click()
    {
        if (hasAuthority)
        {
            CmdClearInstruction();
            CmdAddInstruction("MOVE UP");
            CmdAddInstruction("MOVE UP");
            CmdAddInstruction("MOVE UP");
            CmdAddInstruction("MOVE UP");
            CmdAddInstruction("MOVE RIGHT");
            CmdAddInstruction("MOVE RIGHT");
            CmdAddInstruction("MOVE DOWN");
            CmdAddInstruction("MOVE LEFT");
            CmdAddInstruction("MOVE LEFT");
            CmdAddInstruction("MOVE UP");
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
            Debug.Log("Running instruction: " + instruction);
            
            if (!Instruction.IsValidInstruction(instruction))
                Debug.Log("Robot does not understand instruction: " + instruction); // Later the player should be informed about this
            else if (instruction == Instruction.MoveUp)
                posZ++;
            else if (instruction == Instruction.MoveDown)
                posZ--;
            else if (instruction == Instruction.MoveRight)
                posX++;
            else if (instruction == Instruction.MoveLeft)
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

public static class Instruction
{

    public static string MoveUp { get { return "MOVE UP"; } }
    public static string MoveDown { get { return "MOVE DOWN"; } }
    public static string MoveLeft { get { return "MOVE LEFT"; } }
    public static string MoveRight { get { return "MOVE RIGHT"; } }

    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight
    };

    public static bool IsValidInstruction(string instruction)
    {
        return AllInstructions.Contains(instruction);
    }

    public static bool IsValidInstructionList(List<string> instructions)
    {
        foreach(string instructionString in instructions)
        {
            if (!IsValidInstruction(instructionString))
                return false;
        }

        return true;
    }

}