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
    private bool executingInstructions = false;

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
            CmdAddInstruction("MOVE RIGHT");
            CmdAddInstruction("MOVE RIGHT");
            CmdAddInstruction("MOVE HOME");
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
            else if (instruction == Instruction.MoveHome)
            {
                SanityCheckIsWholeNumber("position X", posX);
                SanityCheckIsWholeNumber("position Z", posZ);
                SanityCheckIsWholeNumber("home X", homeX);
                SanityCheckIsWholeNumber("home Z", homeZ);

                int avoidEndlessWhileCounter = 0;
                while (posX != homeX || posZ != homeZ)
                {
                    avoidEndlessWhileCounter++;
                    if (avoidEndlessWhileCounter > 100000)
                        throw new Exception("Instruction.MoveHome endless while loop. #ProgrammerFail");

                    float difX = Math.Abs(posX - homeX);
                    float difZ = Math.Abs(posZ - homeZ);

                    if (difX >= difZ)
                        posX += GetIncremementOrDecrementToGetCloser(posX, homeX);
                    else
                        posZ += GetIncremementOrDecrementToGetCloser(posZ, homeZ);

                    yield return new WaitForSeconds(1f);
                }
            }

            yield return new WaitForSeconds(1f);
        }

        executingInstructions = false;

        Debug.Log("Finished running instructions for robot");
    }

    private int GetIncremementOrDecrementToGetCloser(float posValue, float homeValue)
    {
        if (posValue > homeValue)
            return -1;
        else if (posValue < homeValue)
            return 1;
        else
            throw new Exception("Should not call this method withot a value difference");
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

    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
    }
}

public static class Instruction
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