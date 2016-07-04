using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Robot : NetworkBehaviour, IAttackable
{

    // ********** COMMON VARIABLES **********

    [SyncVar]
    private float posX;
    [SyncVar]
    private float posZ;

    private float homeX;
    private float homeZ;

    [SyncVar]
    protected int instructionBeingExecuted = 0;
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

    protected SyncListString instructions = new SyncListString();
    [SyncVar]
    private int currentInstructionIndex = 0;

    private List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> Inventory { get { return inventory; } }
    public delegate void InventoryChanged(Robot robot);
    public static event InventoryChanged OnInventoryChanged;

    [SyncVar]
    private int energy;
    public int Energy { get { return energy; } }

    [SyncVar]
    private int health;
    public int Health { get { return health; } }

    // ********** SETTINGS **********

    //public abstract int Settings_CopperCost(); // Might not be needed, because do you need to know the cost of the instance?
    //public abstract int Settings_IronCost(); // Might not be needed, because do you need to know the cost of the instance?
    public abstract int Settings_Memory();
    public abstract int Settings_IPT(); // Instructions Per Tick. Cant call it speed because it can be confused with move speed.
    public abstract int Settings_MaxEnergy();
    public abstract int Settings_InventoryCapacity();
    public abstract int Settings_Damage();
    public abstract int Settings_MaxHealth();

    public List<string> commonInstructions = new List<string>()
    {
        Instructions.MoveUp,
        Instructions.MoveDown,
        Instructions.MoveLeft,
        Instructions.MoveRight,
        Instructions.MoveHome,
        Instructions.LoopStart,
        Instructions.LoopStartPlain,
        Instructions.LoopEnd
    };


    // ********** ABSTRACT METHODS  **********

    protected abstract void Animate();
    public abstract List<string> GetSpecializedInstruction();


    // Use this for initialization
    void Start()
    {
        posX = transform.position.x;
        posZ = transform.position.z;

        homeX = transform.position.x;
        homeZ = transform.position.z;

        energy = Settings_MaxEnergy();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Animate();
    }

    [Client]
    private void Move()
    {
        var newPos = new Vector3(posX, transform.position.y, posZ);
        transform.LookAt(newPos);
        transform.position = Vector3.MoveTowards(transform.position, newPos, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * Settings_IPT());
    }

    [Client]
    public void RunCode(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Settings_Memory())
            {
                SetFeedback("NOT ENOUGH MEMORY"); // TODO: Discuss with BT, here a Client method calls a server method, should not happen? Wont work either.
                return;
            }

            CmdAddInstructions(string.Join(",", newInstructions.ToArray()));
            CmdStartRobot();
            isStarted = true;
        }
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

    public SyncListString GetInstructions()
    {
        return instructions;
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

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("Server: Starting robot");
        if (Settings_IPT() == 1)
            WorldTickController.instance.TickEvent += RunNextInstruction;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.HalfTickEvent += RunNextInstruction;
        else
            throw new Exception("IPT value not supported: " + Settings_IPT());
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
    private void SetFeedback(string message)
    {
        feedback = message;
        instructionBeingExecutedIsValid = false;
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
    private bool IsHome()
    {
        return posX == homeX && posZ == homeZ;
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
            energy = Settings_MaxEnergy();
    }


    [Server]
    private void AddInventoryItem(InventoryItem item)
    {
        if (inventory.Count >= Settings_InventoryCapacity())
        {
            SetFeedback("INVENTORY FULL");
        }

        inventory.Add(item);
        if (OnInventoryChanged != null)
            OnInventoryChanged(this);

        RpcSyncInventory(inventory.Select(i => i.Serialize()).ToArray());
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
    private void ClearInventory()
    {
        inventory = new List<InventoryItem>();
        if (OnInventoryChanged != null)
            OnInventoryChanged(this);

        RpcSyncInventory(inventory.Select(i => i.Serialize()).ToArray());
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] itemStrings)
    {
        inventory = new List<InventoryItem>();

        foreach (string itemString in itemStrings)
        {
            if (itemString == CopperItem.SerializedType)
                inventory.Add(new CopperItem());
            else if (itemString == IronItem.SerializedType)
                inventory.Add(new IronItem());
            else
                throw new Exception("Forgot to add deserialization support for InventoryType: " + itemString);
        }

        if (OnInventoryChanged != null)
            OnInventoryChanged(this);
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
    private PlayerCityController FindPlayerCityControllerOnPosition()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerCity"))
            if (go.transform.position.x == posX && go.transform.position.z == posZ)
                return go.GetComponent<PlayerCityController>();

        return null;
    }

    //[Server]
    //private IAttackable FindAttackableOnPosition()
    //{

    //}

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health >= 0)
            Destroy(gameObject);
    }

}
