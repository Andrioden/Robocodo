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

    private bool isStarted = false;
    public bool IsStarted { get { return isStarted; } }
    private List<InventoryItem> inventory = new List<InventoryItem>();

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
        transform.position = Vector3.MoveTowards(transform.position, newPos, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        visirMeshRenderer.material.color = Color.blue;
    }

    public void RunCode(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            isStarted = true;
            CmdClearInstruction();

            newInstructions.ForEach(x =>
            {
                CmdAddInstruction(x);
            });
             
            CmdStartRobot();
        }
    }

    public List<string> GetInstructions()
    {
        return instructions;
    }

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("Server: Starting robot");
        WorldTickController.instance.TickEvent += RunNextInstruction;
    }

    //[Server]
    //private void StopRobot()
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

    [Server]
    private void RunNextInstruction(object sender)
    {
        string instruction = instructions[currentInstructionIndex];

        Debug.Log("SERVER: Running instruction: " + instruction);

        if (!Instructions.IsValidInstruction(instruction))
            Debug.Log("SERVER: Robot does not understand instruction: " + instruction); // Later the player should be informed about this
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
            SanityCheckIsWholeNumber("position X", posX);
            SanityCheckIsWholeNumber("position Z", posZ);
            SanityCheckIsWholeNumber("home X", homeX);
            SanityCheckIsWholeNumber("home Z", homeZ);

            float difX = Math.Abs(posX - homeX);
            float difZ = Math.Abs(posZ - homeZ);

            if (difX >= difZ)
                posX += GetIncremementOrDecrementToGetCloser(posX, homeX);
            else
                posZ += GetIncremementOrDecrementToGetCloser(posZ, homeZ);

            if (posX == homeX && posZ == homeZ)
                InstructionCompleted();

            return;
        }
        else if (instruction == Instructions.Harvest)
        {
            if (WorldController.instance.world.HasCopperNodeAt((int)posX, (int)posZ))
                inventory.Add(new CopperItem());
            else if (WorldController.instance.world.HasIronNodeAt((int)posX, (int)posZ))
                inventory.Add(new IronItem());
            else
                Debug.LogFormat("SERVER: Robot did not manage to harvest, no resource on {0},{1}", posX, posZ); // Might want to warn player about this, dunno, gameplay decision

        }
        else if (instruction == Instructions.DropInventory)
        {
            Debug.Log("SERVER: Dropping inventory items count: " + inventory.Count);

            PlayerCityController playerCity = FindPlayerCityControllerOnPosition();
            if (playerCity != null)
            {
                Debug.Log("SERVER: Found city, dropping inventory on city");
                playerCity.AddToInventory(inventory);
            }
            else
            {
                Debug.Log("SERVER: No city found, should drop items on ground. Not fully implemented.");
            }

            inventory = new List<InventoryItem>();
        }

        InstructionCompleted();
    }

    [Server]
    private PlayerCityController FindPlayerCityControllerOnPosition()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerCity"))
            if (go.transform.position.x == posX && go.transform.position.z == posZ)
                return go.GetComponent<PlayerCityController>();

        return null;
    }

    [Server]
    private void InstructionCompleted()
    {
        currentInstructionIndex++;

        if (currentInstructionIndex == instructions.Count)
            currentInstructionIndex = 0;
    }

    [Server]
    private int GetIncremementOrDecrementToGetCloser(float posValue, float homeValue)
    {
        if (posValue > homeValue)
            return -1;
        else if (posValue < homeValue)
            return 1;
        else
            throw new Exception("Should not call this method withot a value difference");
    }

    [Server]
    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
    }

    public void Click()
    {
        if (hasAuthority)
            HarvesterRobotSetupPanel.instance.ShowPanel(this);
    }
}

public static class Instructions
{

    public static string MoveUp { get { return "MOVE UP"; } }
    public static string MoveDown { get { return "MOVE DOWN"; } }
    public static string MoveLeft { get { return "MOVE LEFT"; } }
    public static string MoveRight { get { return "MOVE RIGHT"; } }
    public static string MoveHome { get { return "MOVE HOME"; } }
    public static string Harvest { get { return "HARVEST"; } }
    public static string DropInventory { get { return "DROP INVENTORY"; } }

    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        MoveHome,
        Harvest,
        DropInventory
    };

    public static bool IsValidInstruction(string instruction)
    {
        return AllInstructions.Contains(instruction);
    }

    public static bool IsValidInstructionList(List<string> instructions)
    {
        foreach (string instructionString in instructions)
        {
            if (!IsValidInstruction(instructionString))
                return false;
        }

        return true;
    }

}

public abstract class InventoryItem
{
    public abstract string Serialize();
}

public class CopperItem : InventoryItem
{
    public static readonly string SerializedType = "CopperItem";
    public override string Serialize()
    {
        return SerializedType;
    }
}

public class IronItem : InventoryItem
{
    public static readonly string SerializedType = "IronItem";
    public override string Serialize()
    {
        return SerializedType;
    }
}