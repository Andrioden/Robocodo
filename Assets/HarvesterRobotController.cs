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
    private float posX;
    [SyncVar]
    private float posZ;

    private float homeX;
    private float homeZ;

    private List<string> instructions = new List<string>();
    private int currentInstructionIndex = 0;

    private bool robotStarted = false;

    private void Start()
    {
        posX = transform.position.x;
        posZ = transform.position.z;

        homeX = transform.position.x;
        homeZ = transform.position.z;
    }

    // Update is called once per frame
    private void Update()
    {
        //if (isServer)
        //    ExecuteInstructionIfNewTick();

        var newPos = new Vector3(posX, transform.position.y, posZ);
        transform.LookAt(newPos);
        transform.position = Vector3.MoveTowards(transform.position, newPos, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        visirMeshRenderer.material.color = Color.blue;
    }

    public void Click()
    {
        if (hasAuthority && !robotStarted)
        {
            robotStarted = true;
            Debug.Log("Client: Starting robot");
            CmdStartRobot();
        }
    }

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("Server: Starting robot");
        CmdClearInstruction();
        CmdAddInstruction(Instructions.MoveUp);
        CmdAddInstruction(Instructions.MoveUp);
        CmdAddInstruction(Instructions.MoveUp);
        CmdAddInstruction(Instructions.MoveRight);
        CmdAddInstruction(Instructions.MoveRight);
        CmdAddInstruction(Instructions.MoveHome);

        WorldTickController.instance.TickEvent += Server_RunNextInstruction;
    }

    //private void Server_StopRobot()
    //{
    //    Debug.Log("Server: Stopping Robot");
    //    WorldTickController.instance.TickEvent -= Server_RunNextInstruction;
    //}

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

    private void Server_RunNextInstruction(object sender)
    {
        string instruction = instructions[currentInstructionIndex];

        Debug.Log("Running instruction: " + instruction);

        if (!Instructions.IsValidInstruction(instruction))
            Debug.Log("Robot does not understand instruction: " + instruction); // Later the player should be informed about this
        else if (instruction == Instructions.MoveUp)
            posZ++;
        else if (instruction == Instructions.MoveDown)
            posZ--;
        else if (instruction == Instructions.MoveRight)
            posX++;
        else if (instruction == Instructions.MoveLeft)
            posX--;
        else if (instruction == Instructions.MoveHome)
        {
            //Debug.LogFormat("MoveHome - Pos: {0},{1}, Home: {2},{3}", posX, posZ, homeX, homeZ);
            Server_SanityCheckIsWholeNumber("position X", posX);
            Server_SanityCheckIsWholeNumber("position Z", posZ);
            Server_SanityCheckIsWholeNumber("home X", homeX);
            Server_SanityCheckIsWholeNumber("home Z", homeZ);

            float difX = Math.Abs(posX - homeX);
            float difZ = Math.Abs(posZ - homeZ);

            if (difX >= difZ)
                posX += Server_GetIncremementOrDecrementToGetCloser(posX, homeX);
            else
                posZ += Server_GetIncremementOrDecrementToGetCloser(posZ, homeZ);

            if (posX == homeX && posZ == homeZ)
                Server_InstructionCompleted();

            return;
        }

        Server_InstructionCompleted();
    }

    private void Server_InstructionCompleted()
    {
        currentInstructionIndex++;

        if (currentInstructionIndex == instructions.Count)
            currentInstructionIndex = 0;
    }

    private int Server_GetIncremementOrDecrementToGetCloser(float posValue, float homeValue)
    {
        if (posValue > homeValue)
            return -1;
        else if (posValue < homeValue)
            return 1;
        else
            throw new Exception("Should not call this method withot a value difference");
    }

    private void Server_SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
    }
}

public static class Instructions
{

    public static string MoveUp { get { return "MOVE UP"; } }
    public static string MoveDown { get { return "MOVE DOWN"; } }
    public static string MoveLeft { get { return "MOVE LEFT"; } }
    public static string MoveRight { get { return "MOVE RIGHT"; } }
    public static string MoveHome { get { return "MOVE HOME"; } }

    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        MoveHome
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