﻿using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RobotController : NetworkBehaviour, IAttackable, ISelectable, IHasInventory
{
    // ********** COMMON VARIABLES **********
    [SyncVar]
    public string owner = "NEUTRAL";

    [SyncVar]
    protected float x;
    [SyncVar]
    protected float z;

    private float homeX;
    private float homeZ;

    [SyncVar]
    private string feedback = "";
    public string Feedback { get { return feedback; } }

    private IEnumerator feedbackClearCoroutine;

    [SyncVar(hook = "OnIsStartedChanged")]
    private bool isStarted = false;
    public bool IsStarted { get { return isStarted; } }

    public bool isPreviewRobot = false;

    protected SyncListString instructions = new SyncListString();
    [SyncVar]
    private int nextInstructionIndex = 0;

    [SyncVar(hook = "OnCurrentInstructionIndexUpdates")]
    protected int currentInstructionIndex = 0;
    public int CurrentInstructionIndex { get { return currentInstructionIndex; } }

    [SyncVar]
    private bool currentInstructionIndexIsValid = true;
    public bool CurrentInstructionIndexIsValid { get { return currentInstructionIndexIsValid; } }

    [SyncVar]
    private int mainLoopIterationCount = 0;
    public int MainLoopIterationCount { get { return mainLoopIterationCount; } }

    private List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> Inventory { get { return inventory; } }
    public delegate void InventoryChanged(RobotController robot);
    public static event InventoryChanged OnInventoryChanged;

    [SyncVar]
    private bool willSalvageWhenHome = false;
    public bool WillSalvageWhenHome { get { return willSalvageWhenHome; } }
    [SyncVar]
    private bool willReprogramWhenHome = false;
    public bool WillReprogramWhenHome { get { return willReprogramWhenHome; } }

    [SyncVar]
    private int energy;
    public int Energy { get { return energy; } }

    [SyncVar]
    private int health;
    public int Health { get { return health; } }

    // ********** SETTINGS **********

    public abstract string Settings_Name();
    public abstract int Settings_CopperCost();
    public abstract int Settings_IronCost();
    public abstract int Settings_Memory();
    public abstract int Settings_IPT(); // Instructions Per Tick. Cant call it speed because it can be confused with move speed.
    public abstract int Settings_MaxEnergy();
    public abstract int Settings_InventoryCapacity();
    public abstract int Settings_HarvestYield();
    public abstract int Settings_Damage();
    public abstract int Settings_StartHealth();

    private List<string> commonInstructions = new List<string>()
    {
        Instructions.MoveUp,
        Instructions.MoveDown,
        Instructions.MoveLeft,
        Instructions.MoveRight,
        Instructions.MoveHome,
        Instructions.LoopStartNumbered,
        Instructions.LoopStart,
        Instructions.LoopEnd,
        Instructions.DetectThen
    };

    public List<string> CommonInstructions { get { return commonInstructions; } }


    // ********** ABSTRACT METHODS  **********

    protected abstract void Animate();
    public abstract List<string> GetSpecializedInstruction();
    public abstract GameObject SpawnPreviewGameObjectClone();
    protected abstract List<string> GetDefaultInstructions();

    // Use this for initialization
    private void Start()
    {
        InitDefaultValues();
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
        FaceDirection();
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
            StopRobot();
    }

    public void Click()
    {
        if (hasAuthority)
            RobotPanel.instance.Show(this);
    }


    public void InitDefaultValues()
    {
        x = transform.position.x;
        z = transform.position.z;

        homeX = transform.position.x;
        homeZ = transform.position.z;

        energy = Settings_MaxEnergy();
        health = Settings_StartHealth();

        if (instructions.Count == 0)
            SetInstructions(GetDefaultInstructions());
    }

    public Coordinate GetCoordinate()
    {
        return new Coordinate((int)x, (int)z);
    }

    [Command]
    public void CmdToggleSalvageWhenHome()
    {
        willSalvageWhenHome = !willSalvageWhenHome;
        willReprogramWhenHome = false;
    }

    [Command]
    public void CmdToggleReprogramWhenHome()
    {
        willReprogramWhenHome = !willReprogramWhenHome;
        willSalvageWhenHome = false;
    }

    [Client]
    private void Move()
    {
        var newPos = new Vector3(x, transform.position.y, z);
        transform.position = Vector3.MoveTowards(transform.position, newPos, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * Settings_IPT());
    }

    [Client]
    public void FaceDirection()
    {
        Vector3? facePosition = null;

        if (instructions.Count > 0)
        {
            string currentInstruction = instructions[currentInstructionIndex];

            if (currentInstruction == Instructions.AttackUp)
                facePosition = new Vector3(x, transform.position.y, z + 1);
            else if (currentInstruction == Instructions.AttackDown)
                facePosition = new Vector3(x, transform.position.y, z - 1);
            else if (currentInstruction == Instructions.AttackRight)
                facePosition = new Vector3(x + 1, transform.position.y, z);
            else if (currentInstruction == Instructions.AttackLeft)
                facePosition = new Vector3(x - 1, transform.position.y, z);
        }

        if (!facePosition.HasValue)
            facePosition = new Vector3(x, transform.position.y, z);

        transform.LookAt(facePosition.Value);
    }

    private void OnCurrentInstructionIndexUpdates(int newCurrentInstructionIndex)
    {
        currentInstructionIndex = newCurrentInstructionIndex;
        Animate();
    }

    private void OnIsStartedChanged(bool newValue)
    {
        isStarted = newValue;
        RobotPanel.instance.Refresh(this);
    }

    [Client]
    public void RunCode(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Settings_Memory())
            {
                CmdSetFeedback("NOT ENOUGH MEMORY");
                return;
            }

            string instructionsCSV = string.Join(",", newInstructions.ToArray());
            SetInstructions(instructionsCSV);

            if (instructions.Count <= 0)
            {
                CmdSetFeedback("NO INSTRUCTIONS DETECTED");
                return;
            }

            CmdSetInstructions(instructionsCSV);
            CmdStartRobot();

            //For quicker response when changing from setup mode to running mode in GUI. Will be overridden by server when syncvar is synced.
            isStarted = true;
        }
    }

    [Command]
    private void CmdSetInstructions(string instructionsCSV)
    {
        SetInstructions(instructionsCSV);        
    }

    private IEnumerator ClearFeedbackAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        feedback = string.Empty;
    }

    public void SetInstructions(List<string> instructionsList)
    {
        instructions.Clear();

        foreach (string instruction in instructionsList)
            instructions.Add(instruction);
    }

    private void SetInstructions(string instructionsCSV)
    {
        instructions.Clear();

        foreach (string instruction in instructionsCSV.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            instructions.Add(instruction);
    }

    public SyncListString GetInstructions()
    {
        return instructions;
    }

    [Command]
    private void CmdStartRobot()
    {
        Debug.Log("Server: Starting robot");
        isStarted = true;
        if (Settings_IPT() == 1)
            WorldTickController.instance.TickEvent += RunNextInstruction;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.HalfTickEvent += RunNextInstruction;
        else
            throw new Exception("IPT value not supported: " + Settings_IPT());
    }

    /// <summary>
    /// Has to be run without server check because the server variable is not set after network destroy.
    /// It is ok that it runs on the client. Trying to desubscribe a method that isnt subscribed is ok.
    /// </summary>
    private void StopRobot()
    {
        isStarted = false;

        if (Settings_IPT() == 1)
            WorldTickController.instance.TickEvent -= RunNextInstruction;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.HalfTickEvent -= RunNextInstruction;
    }

    public void RunNextInstruction(object sender)
    {
        SetFeedback("");

        if (instructions.Count == 0)
        {
            SetFeedback("NO INSTRUCTIONS");
            return;
        }

        currentInstructionIndexIsValid = true;
        currentInstructionIndex = nextInstructionIndex;
        string instruction = instructions[nextInstructionIndex].Trim();

        //Debug.Log("SERVER: Running instruction: " + instruction);

        if (IsHome())
        {
            energy = Settings_MaxEnergy();

            if (willSalvageWhenHome)
            {
                SalvageRobot();
                return;
            }
            else if (willReprogramWhenHome)
            {
                ReprogramRobot();
                return;
            }
        }

        if (energy <= 0)
        {
            SetFeedback("NOT ENOUGH ENERGY");
            return;
        }
        energy--;

        if (ApplyInstruction(instruction))
            InstructionCompleted();
    }

    [Server]
    private bool ApplyInstruction(string instruction)
    {
        if (instruction == Instructions.DoNothing)
            return true;
        else if (instruction == Instructions.MoveUp)
            ChangePosition(x, z + 1);
        else if (instruction == Instructions.MoveDown)
            ChangePosition(x, z - 1);
        else if (instruction == Instructions.MoveRight)
            ChangePosition(x + 1, z);
        else if (instruction == Instructions.MoveLeft)
            ChangePosition(x - 1, z);
        else if (instruction == Instructions.MoveHome)
        {
            SanityCheckIfPositionNumbersAreWhole();

            float difX = Math.Abs(x - homeX);
            float difZ = Math.Abs(z - homeZ);

            if (difX >= difZ && !IsHome())
                x += GetIncremementOrDecrementToGetCloser(x, homeX);
            else if (difX < difZ)
                z += GetIncremementOrDecrementToGetCloser(z, homeZ);

            if (!IsHome())
                return false;
        }
        else if (Instructions.IsValidLoopStart(instruction))
        {
            IterateLoopStartCounterIfNeeded(instruction);
            ResetAllInnerLoopStarts(nextInstructionIndex + 1);
        }
        else if (instruction == Instructions.LoopEnd)
            SetInstructionToMatchingLoopStart();
        else if (instruction == Instructions.Harvest && !isPreviewRobot)
        {
            for (int i = 0; i < Settings_HarvestYield(); i++)
            {
                if (Settings_InventoryCapacity() == 0)
                    SetFeedback("NO INVENTORY CAPACITY");
                else if (IsInventoryFull())
                    SetFeedback("INVENTORY FULL");
                else if (WorldController.instance.HarvestFromNode(CopperItem.SerializedType, x, z))
                    TransferToInventory(new CopperItem());
                else if (WorldController.instance.HarvestFromNode(IronItem.SerializedType, x, z))
                    TransferToInventory(new IronItem());
                else
                    SetFeedback("NOTHING TO HARVEST");
            }

            if (Settings_HarvestYield() == 0)
                SetFeedback("NO HARVEST YIELD");
        }
        else if (instruction == Instructions.DropInventory && !isPreviewRobot)
            DropInventory();
        else if (instruction == Instructions.AttackMelee && !isPreviewRobot)
            AttackPosition(x, z);
        else if (instruction == Instructions.AttackUp && !isPreviewRobot)
            AttackPosition(x, z + 1);
        else if (instruction == Instructions.AttackDown && !isPreviewRobot)
            AttackPosition(x, z - 1);
        else if (instruction == Instructions.AttackRight && !isPreviewRobot)
            AttackPosition(x + 1, z);
        else if (instruction == Instructions.AttackLeft && !isPreviewRobot)
            AttackPosition(x - 1, z);
        else if (Instructions.IsValidDetect(instruction) && !isPreviewRobot)
        {
            string detectionSource = instruction.Split(' ')[1];
            if (detectionSource == "ENEMY" && FindNearbyEnemy((int)x, (int)z, 3.0) == null)
                return true;
            else if (detectionSource == "FULL" && IsInventoryFull())
                return true;
            else if (detectionSource == "IRON" || detectionSource == "COPPER")
            {
                SetFeedback("Cant detect IRON or COPPER yet. Not implemented");
                return true;
            }

            Debug.Log("Nearby: " + FindNearbyEnemy((int)x, (int)z, 4.3));

            string detectInstruction = Instructions.GetStringAfterSpace(instruction, 3);
            return ApplyInstruction(detectInstruction);
        }
        else
            SetFeedback(string.Format("UNKNOWN INSTRUCTION: '{0}'", instruction));

        return true;
    }

    [Server]
    private void SetFeedback(string message)
    {
        feedback = message;
        currentInstructionIndexIsValid = false;
    }

    [Command]
    private void CmdSetFeedback(string message)
    {
        SetFeedback(message);

        /* When client sets feedback we have to clear it manually as clearing on server is based on tick */
        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = ClearFeedbackAfterSeconds(1f);
        StartCoroutine(feedbackClearCoroutine);
    }

    private void ChangePosition(float newPosX, float newPosZ)
    {
        if (newPosX >= WorldController.instance.Width || newPosX < 0 || newPosZ >= WorldController.instance.Height || newPosZ < 0)
            SetFeedback("CAN NOT MOVE THERE");
        else
        {
            x = newPosX;
            z = newPosZ;
        }
    }

    private void SanityCheckIfPositionNumbersAreWhole()
    {
        SanityCheckIsWholeNumber("position X", x);
        SanityCheckIsWholeNumber("position Z", z);
        SanityCheckIsWholeNumber("home X", homeX);
        SanityCheckIsWholeNumber("home Z", homeZ);
    }

    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
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

    private bool IsHome()
    {
        return x == homeX && z == homeZ;
    }

    private void InstructionCompleted()
    {
        nextInstructionIndex++;

        if (nextInstructionIndex == instructions.Count)
        {
            nextInstructionIndex = 0;
            ResetAllInnerLoopStarts(nextInstructionIndex);
            mainLoopIterationCount++;
        }
    }

    private void IterateLoopStartCounterIfNeeded(string instruction)
    {
        if (instruction == Instructions.LoopStart)
            return;

        string loopNumber = instruction.Replace(Instructions.LoopStart, "").Trim();
        string[] loopNumberSplit = loopNumber.Split('/');

        int currentLoopCount = -1;
        int totalLoopCount = -1;

        if (loopNumberSplit.Length == 1)
        {
            // First time running Loop
            currentLoopCount = 1;
            totalLoopCount = Convert.ToInt32(loopNumberSplit[0]);
        }
        else if (loopNumberSplit.Length == 2)
        {
            // Loop has been run before, example 'LOOP START (1/2)' means that it has been run 1 of 2 times
            currentLoopCount = Convert.ToInt32(loopNumberSplit[0]) + 1;
            totalLoopCount = Convert.ToInt32(loopNumberSplit[1]);
        }
        else
            throw new Exception("Illegal amount of forward slashes in instruction: " + instruction);

        instructions[nextInstructionIndex] = Instructions.LoopStartNumberedSet(currentLoopCount, totalLoopCount);
        instructions.Dirty(nextInstructionIndex);
    }

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

    private void SetInstructionToMatchingLoopStart()
    {
        int skippingLoopStarts = 0;
        for (int i = nextInstructionIndex - 1; i >= 0; i--)
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
                        nextInstructionIndex = i - 1;
                        return;
                    }
                }
                else
                    skippingLoopStarts--;
            }
        }

        SetFeedback("COULD NOT FIND MATCHING LOOP START");
    }

    [Server]
    private void DropInventory()
    {
        Debug.Log("SERVER: Dropping inventory items count: " + inventory.Count);

        IHasInventory droppableTarget = FindDroppableTarget((int)x, (int)z);
        if (droppableTarget != null)
        {
            Debug.Log("SERVER: Found something to drop on, dropping inventory on it");
            List<InventoryItem> itemsNotAdded = droppableTarget.TransferToInventory(inventory);
            SetInventory(itemsNotAdded);
            if (itemsNotAdded.Count > 0)
                SetFeedback("NOT ALL ITEMS DROPPED, TARGET FULL");
        }
        else
            Debug.Log("SERVER: No droppable, should drop items on ground. Not fully implemented.");
    }

    /// <summary>
    /// Returns true if the item was added successfully
    /// </summary>
    [Server]
    private bool TransferToInventory(InventoryItem item)
    {
        List<InventoryItem> notAddedItems = TransferToInventory(new List<InventoryItem> { item });
        return notAddedItems.Count == 0;
    }

    [Server]
    public List<InventoryItem> TransferToInventory(List<InventoryItem> items)
    {
        List<InventoryItem> notAdded = new List<InventoryItem>();
        foreach (InventoryItem item in items)
        {
            if (IsInventoryFull())
                notAdded.Add(item);
            else
                inventory.Add(item);
        }

        if (OnInventoryChanged != null)
            OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.SerializeList(inventory));

        return notAdded;
    }

    [Server]
    private void SetInventory(List<InventoryItem> items)
    {
        inventory = items;
        if (OnInventoryChanged != null)
            OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.SerializeList(inventory));
    }

    [Server]
    private bool IsInventoryFull()
    {
        return inventory.Count >= Settings_InventoryCapacity();
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] itemCounts)
    {
        inventory = InventoryItem.DeserializeList(itemCounts);

        if (OnInventoryChanged != null)
            OnInventoryChanged(this);
    }

    [Server]
    private void AttackPosition(float x, float z)
    {
        IAttackable attackable = FindAttackableEnemy((int)x, (int)z);
        if (attackable != null)
            attackable.TakeDamage(Settings_Damage());
        else
            SetFeedback("NO TARGET TO ATTACK");
    }

    /// <summary>
    /// TODO: Should probably rewrite to use collisions and remove tag.
    /// </summary>
    /// <returns></returns>
    [Server]
    private PlayerCityController FindPlayerCityControllerOnPosition()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerCity"))
            if (go.transform.position.x == x && go.transform.position.z == z)
                return go.GetComponent<PlayerCityController>();

        return null;
    }

    /// <summary>
    /// Finds go that can be dropped on based on colliders. Requires that the collider is hierarchaly 1 step below the IAttackable script.
    /// </summary>
    [Server]
    private IHasInventory FindDroppableTarget(int x, int z)
    {
        foreach (GameObject potentialGO in FindNearbyCollidingGameObjects())
        {
            IHasInventory droppable = potentialGO.transform.root.GetComponent<IHasInventory>();
            if (droppable != null && potentialGO.transform.position.x == x && potentialGO.transform.position.z == z)
                return droppable;
        }

        Debug.Log("Did not find attackable");
        return null;
    }

    /// <summary>
    /// Finds attackable enemy based on colliders. Requires that the collider is hierarchaly 1 step below the IAttackable script.
    /// </summary>
    [Server]
    private IAttackable FindAttackableEnemy(int x, int z)
    {
        foreach (GameObject potentialGO in FindNearbyCollidingGameObjects())
        {
            IAttackable attackable = potentialGO.transform.root.GetComponent<IAttackable>();

            if (attackable != null && potentialGO.transform.position.x == x && potentialGO.transform.position.z == z)
            {
                if (attackable.GetOwner() != GetOwner())
                    return attackable;
                else
                    Debug.Log("Found attackable but it was friendly");
            }
        }

        Debug.Log("Did not find attackable");
        return null;
    }

    /// <summary>
    /// Finds nearby enemy based on colliders. Requires that the collider is hierarchaly 1 step below the IAttackable script.
    /// </summary>
    [Server]
    private IAttackable FindNearbyEnemy(int x, int z, double maxDistance)
    {
        foreach (GameObject potentialGO in FindNearbyCollidingGameObjects())
        {
            IAttackable attackable = potentialGO.transform.root.GetComponent<IAttackable>();
            if (attackable == null)
                continue;

            int toX = (int)potentialGO.transform.position.x;
            int toZ = (int)potentialGO.transform.position.z;
            if (MathUtils.Distance(x, z, toX, toZ) <= maxDistance)
            {
                if (attackable.GetOwner() != GetOwner())
                    return attackable;
            }
        }

        Debug.Log("Nothing nearby");
        return null;
    }

    [Server]
    private List<GameObject> FindNearbyCollidingGameObjects()
    {
        return Physics.OverlapSphere(transform.position, 7.0f /*Radius*/)
             .Except(new[] { GetComponent<Collider>() })
             .Where(c => c.transform.root.gameObject != gameObject )
             .Select(c => c.gameObject)
             .ToList();
    }

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.LogFormat("Robot {0} took {1} damage and now has {2} health", name, damage, health);

        if (health <= 0)
            NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void SalvageRobot()
    {
        if (isPreviewRobot) // TODO: REMOVE. This code can be removed when testing is done because it should never be reached by the preview code
            return;

        PlayerCityController playerCity = FindPlayerCityControllerOnPosition();

        List<InventoryItem> salvagedResources = new List<InventoryItem>();

        int salvagedCopper = MathUtils.RoundMin1IfHasValue(Settings_CopperCost() * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedCopper; c++)
            salvagedResources.Add(new CopperItem());

        int salvagedIron = MathUtils.RoundMin1IfHasValue(Settings_IronCost() * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedIron; c++)
            salvagedResources.Add(new IronItem());

        playerCity.TransferToInventory(salvagedResources);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ReprogramRobot()
    {
        if (isPreviewRobot) // TODO: REMOVE. This code can be removed when testing is done because it should never be reached by the preview code
            return;

        willReprogramWhenHome = false;

        PlayerCityController playerCity = FindPlayerCityControllerOnPosition();

        int reprogramCopperCost = MathUtils.RoundMin1IfHasValue(Settings_CopperCost() * Settings.Robot_ReprogramPercentage / 100.0);
        int reprogramIronCost = MathUtils.RoundMin1IfHasValue(Settings_IronCost() * Settings.Robot_ReprogramPercentage / 100.0);

        if (playerCity.GetCopperCount() >= reprogramCopperCost && playerCity.GetIronCount() >= reprogramIronCost)
        {
            playerCity.RemoveResources(reprogramCopperCost, reprogramIronCost);

            StopRobot();
            currentInstructionIndex = 0;
            currentInstructionIndexIsValid = true;
            mainLoopIterationCount = 0;
        }
        else
            SetFeedback("NOT ENOUGH RESOURCES TO REPROGRAM");
    }

    public string GetOwner()
    {
        return owner;
    }

    public void PreviewResetRobot()
    {
        x = homeX;
        z = homeZ;
        transform.position = new Vector3(x, 1, z);
        energy = Settings_MaxEnergy();
        currentInstructionIndex = 0;
        nextInstructionIndex = 0;
        mainLoopIterationCount = 0;
    }

}
