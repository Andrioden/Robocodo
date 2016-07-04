using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;
using System.Linq;

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

    [SyncVar]
    private int energy;
    public int Energy { get { return energy; } }


    // SETTINGS
    public static int CopperCost = 1;
    public static int IronCost = 3;
    public static int Memory = 20;
    public static int IPT = 1; // Instructions Per Tick. Cant call it speed because it can be confused with move speed.
    public static int MaxEnergy = 50;
    public static int InventoryCapacity = 2;

    private void Start()
    {
        posX = transform.position.x;
        posZ = transform.position.z;

        homeX = transform.position.x;
        homeZ = transform.position.z;

        energy = MaxEnergy;
    }

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
    public void RunCode(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Memory) { 
                SetFeedback("NOT ENOUGH MEMORY");
                return;
            }

            CmdAddInstructions(string.Join(",", newInstructions.ToArray()));
            CmdStartRobot();
        }
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

        //List<string> demoInstructions = new List<string>()
        //{
        //    Instructions.MoveLeft,
        //    Instructions.LoopStartPlain,
        //        Instructions.LoopStartNumbered(0, 2),
        //            Instructions.MoveUp,
        //            Instructions.LoopStartNumbered(0, 1),
        //                Instructions.MoveRight,
        //            Instructions.LoopEnd,
        //            Instructions.LoopStartNumbered(0, 1),
        //                Instructions.MoveRight,
        //            Instructions.LoopEnd,
        //        Instructions.LoopEnd,
        //    Instructions.LoopEnd,
        //    Instructions.MoveLeft
        //};

        return string.Join("\n", demoInstructions.ToArray());
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

        isStarted = true;
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
    private void CmdAddInstructions(string instructionsCSV)
    {
        List<string> newInstructions = instructionsCSV.Split(',').ToList();

        newInstructions.ForEach(x =>
        {
            instructions.Add(x);
        });
    }

    [Server]
    private void RunNextInstruction(object sender)
    {
        instructionBeingExecutedIsValid = true;
        instructionBeingExecuted = currentInstructionIndex;
        string instruction = instructions[currentInstructionIndex];

        if (energy <= 0)
        {
            SetFeedback("NOT ENOUGH ENERGY");
            return;
        }
        energy--;

        Debug.Log("SERVER: Running instruction: " + instruction);

        if (instruction == Instructions.MoveUp)
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

            if (IsHome())
                InstructionCompleted();

            return;
        }
        else if (instruction == Instructions.Harvest)
        {
            if (WorldController.instance.HarvestFromNode(CopperItem.SerializedType, posX, posZ))
                AddInventoryItem(new CopperItem());
            else if (WorldController.instance.HarvestFromNode(IronItem.SerializedType, posX, posZ))
                AddInventoryItem(new IronItem());
            else
                SetFeedback("NOTHING TO HARVEST");
        }
        else if (instruction == Instructions.DropInventory)
            DropInventory();
        else if (Instructions.IsValidLoopStart(instruction))
        {
            IterateLoopStartCounterIfNeeded(instruction);
            ResetAllInnerLoopStarts(currentInstructionIndex + 1);
        }
        else if (instruction == Instructions.LoopEnd)
            SetInstructionToMatchingLoopStart();
        else
        {
            Debug.Log("SERVER: Robot does not understand instruction: " + instruction);
            SetFeedback("UNKNOWN COMMAND");
        }

        InstructionCompleted();
    }

    [Server]
    private void DropInventory()
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

    [Server]
    private void IterateLoopStartCounterIfNeeded(string instruction)
    {
        if (instruction == Instructions.LoopStartPlain)
            return;

        string paraContent = Instructions.GetParenthesesContent(instruction);
        string[] paraContentSlashSplit = paraContent.Split('/');

        int currentLoopCount = -1;
        int totalLoopCount = -1;

        if (paraContentSlashSplit.Length == 1)
        {
            // First time running Loop
            currentLoopCount = 1;
            totalLoopCount = Convert.ToInt32(paraContentSlashSplit[0]);
        }
        else if (paraContentSlashSplit.Length == 2)
        {
            // Loop has been run before, example 'LOOP START (1/2)' means that it has been run 1 of 2 times
            currentLoopCount = Convert.ToInt32(paraContentSlashSplit[0]) + 1;
            totalLoopCount = Convert.ToInt32(paraContentSlashSplit[1]);
        }
        else
            throw new Exception("Illegal amount of forward slashes in instruction: " + instruction);

        instructions[currentInstructionIndex] = Instructions.LoopStartNumbered(currentLoopCount, totalLoopCount);
    }

    [Server]
    private void ResetAllInnerLoopStarts(int startingIndex)
    {
        int loopEndSkippingUntilDone = 0;
        for (int i = startingIndex; i < instructions.Count; i++)
        {
            if (Instructions.IsValidLoopStart(instructions[i]))
            {
                loopEndSkippingUntilDone++;
                instructions[i] = Instructions.LoopStartReset(instructions[i]);
            }
            else if (instructions[i] == Instructions.LoopEnd)
            {
                if (loopEndSkippingUntilDone == 0)
                    return;
                else
                    loopEndSkippingUntilDone--;
            }
        }
    }

    [Server]
    private void SetInstructionToMatchingLoopStart()
    {
        int skippingLoopStarts = 0;
        for (int i = currentInstructionIndex - 1; i >= 0; i--)
        {
            if (instructions[i] == Instructions.LoopEnd)
                skippingLoopStarts++;
            else if (Instructions.IsValidLoopStart(instructions[i]))
            {
                if (skippingLoopStarts == 0)
                {
                    if (Instructions.IsLoopStartCompleted(instructions[i]))
                        return;
                    else
                    {
                        currentInstructionIndex = i - 1;
                        return;
                    }
                }
                else
                    skippingLoopStarts--;
            }
        }

        SetFeedback("Could not find matching LOOP START");
    }

    [Server]
    private void InstructionCompleted()
    {
        currentInstructionIndex++;

        if (currentInstructionIndex == instructions.Count)
        {
            currentInstructionIndex = 0;
            ResetAllInnerLoopStarts(currentInstructionIndex);
        }

        if (IsHome())
            energy = MaxEnergy;
    }

    [Server]
    private bool IsHome()
    {
        return posX == homeX && posZ == homeZ;
    }

    [Server]
    private void AddInventoryItem(InventoryItem item)
    {
        if (inventory.Count >= InventoryCapacity)
        {
            SetFeedback("INVENTORY FULL");
        }

        inventory.Add(item);
        if (OnInventoryChanged != null)
            OnInventoryChanged(this);
    }

    [Server]
    private void ClearInventory()
    {
        inventory = new List<InventoryItem>();
        if (OnInventoryChanged != null)
            OnInventoryChanged(this);
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

    [Server]
    private void SetFeedback(string message)
    {
        feedback = message;
        instructionBeingExecutedIsValid = false;
    }

}