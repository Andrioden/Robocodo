using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

public class HarvesterRobotController : NetworkBehaviour, ISelectable
{
    public MeshRenderer visirMeshRenderer;

    [SyncVar]
    private float posX;
    [SyncVar]
    private float posZ;

    [SyncVar]
    private int instructionBeingExecuted = 0;
    public int InstructionBeingExecuted { get { return instructionBeingExecuted; } }

    [SyncVar]
    private bool instructionBeingExecutedIsValid = true;
    public bool InstructionBeingExecutedIsValid { get { return instructionBeingExecutedIsValid; } }

    [SyncVar]
    private string feedback = "";
    public string Feedback { get { return feedback; } }

    [SyncVar]
    private bool isStarted = false;
    public bool IsStarted { get { return isStarted; } }

    private SyncListString instructions = new SyncListString();
    [SyncVar]
    private int currentInstructionIndex = 0;

    private float homeX;
    private float homeZ;

    private List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> Inventory { get { return inventory; } }

    public delegate void InventoryChanged(HarvesterRobotController robot);
    public static event InventoryChanged OnInventoryChanged;

    // SETTINGS
    public static int CopperCost = 1;
    public static int IronCost = 3;
    public static int Memory = 10;
    public static int InventoryCapacity = 2;
    public static int IPT = 1; // Instructions Per Tick. Cant call it speed because it can be confused with move speed.

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
        transform.position = Vector3.MoveTowards(transform.position, newPos, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * IPT);
    }

    public void Click()
    {
        if (hasAuthority)
            RobotPanel.instance.ShowPanel(this);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        visirMeshRenderer.material.color = Color.blue;
    }

    [Client]
    public bool RunCode(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Memory)
            {
                feedback = "NOT ENOUGH MEMORY";
                return false;
            }

            isStarted = true;
            newInstructions.ForEach(x =>
            {
                CmdAddInstruction(x); // TODO: Do more effective, cant we add all at once? string list, csv?
            });

            CmdStartRobot();
            return true;
        }

        return false;
    }

    public SyncListString GetInstructions()
    {
        return instructions;
    }

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("Server: Starting robot");
        if (IPT == 1)
            WorldTickController.instance.TickEvent += RunNextInstruction;
        else if (IPT == 2)
            WorldTickController.instance.HalfTickEvent += RunNextInstruction;
        else
            throw new Exception("IPT value not supported: " + IPT);
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

    [Server]
    private void RunNextInstruction(object sender)
    {
        instructionBeingExecutedIsValid = true;
        instructionBeingExecuted = currentInstructionIndex;
        string instruction = instructions[currentInstructionIndex];

        Debug.Log("SERVER: Running instruction: " + instruction);

        if (!Instructions.IsValidInstruction(instruction))
        {
            Debug.Log("SERVER: Robot does not understand instruction: " + instruction);
            instructionBeingExecutedIsValid = false;
            feedback = "Unknown command";
        }
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
            SanityCheckIfPositionNumbersAreWhole();

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
            if (WorldController.instance.HarvestFromNode(CopperItem.SerializedType, posX, posZ))
                AddInventoryItem(new CopperItem());
            else if (WorldController.instance.HarvestFromNode(IronItem.SerializedType, posX, posZ))
                AddInventoryItem(new IronItem());


            //if (WorldController.instance.world.HasCopperNodeAt((int)posX, (int)posZ))
            //    if (!AddInventoryItem(new CopperItem()))
            //        instructionBeingExecutedIsValid = false;
            //    else if (WorldController.instance.world.HasIronNodeAt((int)posX, (int)posZ))
            //    {
            //        if (!AddInventoryItem(new IronItem()))
            //            instructionBeingExecutedIsValid = false;
            //        else {
            //            Debug.LogFormat("SERVER: Robot did not manage to harvest, no resource on {0},{1}", posX, posZ); // Might want to warn player about this, dunno, gameplay decision
            //            instructionBeingExecutedIsValid = false;
            //            feedback = "NOTHING TO HARVEST";
            //        }
            //    }
        }
        else if (instruction == Instructions.DropInventory)
        {
            Debug.Log("SERVER: Dropping inventory items count: " + inventory.Count);

            PlayerCityController playerCity = FindPlayerCityControllerOnPosition();
            if (playerCity != null)
            {
                Debug.Log("SERVER: Found city, dropping inventory on city");
                playerCity.AddToInventory(inventory);
                ClearInventory();
            }
            else
            {
                Debug.Log("SERVER: No city found, should drop items on ground. Not fully implemented.");
            }
        }

        InstructionCompleted();
    }

    [Server]
    private bool AddInventoryItem(InventoryItem item)
    {
        if (inventory.Count >= InventoryCapacity)
        {
            feedback = "INVENTORY FULL";
            return false;
        }

        inventory.Add(item);
        OnInventoryChanged(this);
        return true;
    }

    [Server]
    private void ClearInventory()
    {
        inventory = new List<InventoryItem>();
        //if (OnInventoryChanged != null)
        //    OnInventoryChanged(this);
    }

    public string GetDemoInstructions()
    {
        List<string> demoInstructions = new List<string>()
        {
            Instructions.MoveUp,
            Instructions.Harvest,
            Instructions.MoveHome,
            Instructions.DropInventory
        };

        return string.Join("\n", demoInstructions.ToArray());
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
    private void SanityCheckIfPositionNumbersAreWhole()
    {
        SanityCheckIsWholeNumber("position X", posX);
        SanityCheckIsWholeNumber("position Z", posZ);
        SanityCheckIsWholeNumber("home X", homeX);
        SanityCheckIsWholeNumber("home Z", homeZ);
    }

    [Server]
    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
    }

}