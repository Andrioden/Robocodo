using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RobotController : Unit, IAttackable, ISelectable, IHasInventory
{
    // ********** COMMON VARIABLES **********

    public GameObject meshGO;

    public Renderer[] colorRenderers;
    private bool isColorSet = false;

    private bool isAlreadyHome = false;

    [SyncVar]
    private string feedback = "";
    public string Feedback { get { return feedback; } }

    private IEnumerator feedbackClearCoroutine;

    [SyncVar(hook = "OnIsStartedUpdated")]
    private bool isStarted = false;
    public bool IsStarted { get { return isStarted; } }

    public bool isPreviewRobot = false;

    protected List<Instruction> instructions = new List<Instruction>();
    public List<Instruction> Instructions { get { return instructions; } }
    public static event Action<RobotController> OnInstructionsChanged = delegate { };
    public void NotifyInstructionsChanged() { OnInstructionsChanged(this); }

    [SyncVar]
    public int nextInstructionIndex;

    [SyncVar]
    protected int currentInstructionIndex;
    public int CurrentInstructionIndex { get { return currentInstructionIndex; } }

    [SyncVar]
    private bool currentInstructionIndexIsValid = true;
    public bool CurrentInstructionIndexIsValid { get { return currentInstructionIndexIsValid; } }

    protected Instruction lastAppliedInstruction;
    protected Instruction LastAppliedInstruction
    {
        get { return lastAppliedInstruction; }
        set
        {
            lastAppliedInstruction = value;
            if (isServer)
                RpcSyncLastAppliedInstruction(lastAppliedInstruction.Serialize());
            else if (isClient)
                Debug.LogError("Should not set variable LastAppliedInstruction on client");
        }
    }

    [SyncVar]
    public bool lastAttackedTargetWasAnHit;
    [SyncVar]
    public int lastAttackedTargetX;
    [SyncVar]
    public int lastAttackedTargetZ;

    private List<Instruction> _allowedInstructions = new List<Instruction>();

    [SyncVar]
    private int mainLoopIterationCount;
    public int MainLoopIterationCount { get { return mainLoopIterationCount; } }

    private List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> Inventory { get { return inventory; } }
    public static event Action<RobotController> OnInventoryChanged = delegate { };

    private List<Module> modules = new List<Module>();
    public List<Module> Modules { get { return modules; } }
    public static event Action<RobotController> OnModulesChanged = delegate { };

    [SyncVar]
    private bool willSalvageWhenHome = false;
    public bool WillSalvageWhenHome { get { return willSalvageWhenHome; } }
    [SyncVar]
    private bool willReprogramWhenHome = false;
    public bool WillReprogramWhenHome { get { return willReprogramWhenHome; } }

    private bool isReprogrammingRobot = false;
    private int currentInstructionBeingCleared = 0;
    private int currentInstructionClearTickCounter = 0;

    [SyncVar]
    protected int energy;
    public int Energy { get { return energy; } }

    // ********** SETTINGS **********

    public abstract string Settings_Name();
    public abstract Color Settings_Color();
    public abstract Cost Settings_Cost();
    public abstract int Settings_Memory();
    public abstract int Settings_IPT(); // Instructions Per Tick. Cant call it speed because it can be confused with move speed.
    public abstract int Settings_MaxEnergy();
    public abstract int Settings_InventoryCapacity();
    public abstract int Settings_ModuleCapacity();
    public abstract int Settings_HarvestYield();
    public abstract Sprite Sprite();

    private List<Instruction> commonInstructions = new List<Instruction>()
    {
        new Instruction_Idle(),
        new Instruction_Move(MoveDirection.Up),
        new Instruction_Move(MoveDirection.Down),
        new Instruction_Move(MoveDirection.Left),
        new Instruction_Move(MoveDirection.Right),
        new Instruction_Move(MoveDirection.Random),
        new Instruction_Move(MoveDirection.Home),
        new Instruction_LoopStart(),
        new Instruction_LoopEnd(),
        new Instruction_DetectThen(DetectSource.Enemy, null),
        new Instruction_DetectThen(DetectSource.Full, null),
    };

    public List<Instruction> CommonInstructions { get { return commonInstructions; } }


    // ********** ABSTRACT METHODS  **********

    public abstract List<Instruction> GetSpecializedInstructions();
    protected abstract List<Instruction> GetSuggestedInstructionSet();
    public abstract GameObject SpawnPreviewGameObjectClone();
    protected abstract void Animate();

    // Use this for initialization
    private void Start()
    {
        if (!meshGO)
            Debug.LogError("Mesh game object reference missing. Will not be able to hide physical robot when in garage etc.");

        x = transform.position.x;
        z = transform.position.z;

        if (instructions.Count == 0)
            SetInstructions(GetSuggestedInstructionSet());

        if (isServer)
            InitDefaultValues();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isColorSet)
            SetMaterialColor();

        if (Owner != null && Owner.City != null)
            EnterExitGarageCheck();

        Move();
        Face();
        Animate();
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            StopTicking();
            if (isServer)
                modules.ForEach(module => module.Uninstall());
        }
    }

    public void Click()
    {
        if (hasAuthority)
            RobotPanel.instance.Show(this);
    }

    public ClickablePriority ClickPriority()
    {
        return ClickablePriority.Medium;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitDefaultValues()
    {
        energy = Settings_MaxEnergy();
        health = Settings_StartHealth();
        currentInstructionIndex = 0;
        nextInstructionIndex = 0;
        mainLoopIterationCount = 0;

        if (_allowedInstructions.Count == 0)
            CacheAllowedInstructions();
    }

    private void EnterExitGarageCheck()
    {
        if (IsHomeByTransform() && !isAlreadyHome)
        {
            isAlreadyHome = true;
            meshGO.SetActive(false);
            Owner.City.EnterGarage(this);
        }
        else if (!IsHomeByTransform() && isAlreadyHome)
        {
            isAlreadyHome = false;
            meshGO.SetActive(true);
            Owner.City.ExitGarage(this);
        }
        else if (IsHomeByTransform() && isAlreadyHome && MouseManager.currentlySelected == gameObject && !isStarted)
            meshGO.SetActive(true);
        else if (IsHomeByTransform() && isAlreadyHome && MouseManager.currentlySelected != this && !isStarted)
            meshGO.SetActive(false);
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
        var newPosition = new Vector3(x, transform.position.y, z);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * Settings_IPT());
    }

    [Client]
    private void Face()
    {
        if (LastAppliedInstruction == null)
            return;

        Vector3? faceVector = null;

        if (LastAppliedInstruction.GetType() == typeof(Instruction_Move))
            faceVector = new Vector3(x, transform.position.y, z);
        else if (LastAppliedInstruction.GetType() == typeof(Instruction_Attack) && lastAttackedTargetWasAnHit)
            faceVector = new Vector3(lastAttackedTargetX, transform.position.y, lastAttackedTargetZ);

        if (faceVector.HasValue)
            transform.LookAt(faceVector.Value);
    }

    [ClientRpc]
    private void RpcSyncLastAppliedInstruction(string instructionString)
    {
        lastAppliedInstruction = InstructionsHelper.Deserialize(instructionString);
    }

    [Client]
    private void OnIsStartedUpdated(bool newValue)
    {
        isStarted = newValue;

        if (Owner != null)
        {
            RobotPanel.instance.Refresh(this);
        }
    }

    [Client]
    protected bool ShouldAnimationBePlayed()
    {
        return (energy > 0) && IsStarted && currentInstructionIndexIsValid && !isReprogrammingRobot && (LastAppliedInstruction != null);
    }

    [Client]
    public void Run(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Settings_Memory())
            {
                SetFeedback("NOT ENOUGH MEMORY", true, true);
                return;
            }

            SetInstructions(newInstructions);

            if (instructions.Count <= 0)
            {
                SetFeedback("NO INSTRUCTIONS DETECTED", true, true);
                return;
            }

            CmdSetInstructions(InstructionsHelper.SerializeList(instructions));
            CmdStartRobot();

            //For quicker response when changing from setup mode to running mode in GUI. Will be overridden by server when syncvar is synced.
            isStarted = true;
        }
    }

    [Server]
    public void SetInstructionsAndSyncToOwner(List<Instruction> newInstructions)
    {
        instructions = newInstructions;
        TargetSetInstructions(Owner.connectionToClient, InstructionsHelper.SerializeList(newInstructions));
    }

    [TargetRpc]
    private void TargetSetInstructions(NetworkConnection target, string[] newInstructions)
    {
        instructions = InstructionsHelper.Deserialize(newInstructions.ToList());
    }

    [Command]
    private void CmdSetInstructions(string[] newInstructions)
    {
        instructions = InstructionsHelper.Deserialize(newInstructions.ToList());
    }

    [Client]
    public void SetInstructions(List<Instruction> newInstructions)
    {
        instructions = newInstructions;
    }

    [Client]
    public void SetInstructions(List<string> newInstructions)
    {
        instructions = InstructionsHelper.Deserialize(newInstructions);
    }

    public void CacheAllowedInstructions()
    {
        _allowedInstructions = commonInstructions
            .Concat(GetSpecializedInstructions())
            .Concat(GetAllModuleInstructions())
            .ToList();
    }

    private List<Instruction> GetAllModuleInstructions()
    {
        return modules.Select(m => m.GetInstructions()).SelectMany(x => x).ToList();
    }

    [Command]
    public void CmdStartRobot()
    {
        isStarted = true;

        if (Settings_IPT() == 1)
            WorldTickController.instance.OnTick += Tick;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.OnHalfTick += Tick;
        else
            throw new Exception("IPT value not supported: " + Settings_IPT());
    }

    [Server]
    private void StopRobot()
    {
        isStarted = false;
        StopTicking();
    }

    /// <summary>
    /// Might be run by client, thats ok. Desubscribing a method that was never subcribed is OK.
    /// </summary>
    private void StopTicking()
    {
        if (Settings_IPT() == 1)
            WorldTickController.instance.OnTick -= Tick;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.OnHalfTick -= Tick;
    }

    /// <summary>
    /// Is not set to [Server] because it is used by the movement previewer
    /// </summary>
    public void Tick()
    {
        SetFeedback("", false, true);

        if (instructions.Count == 0)
        {
            SetFeedback("NO INSTRUCTIONS", true, true);
            return;
        }

        currentInstructionIndexIsValid = true;
        currentInstructionIndex = nextInstructionIndex;
        Instruction currentInstruction = instructions[nextInstructionIndex];

        IEnergySource energySource = FindFirstOnCurrentPosition<IEnergySource>();
        if (energySource != null)
        {
            if (isPreviewRobot)
                AddEnergy(Settings_MaxEnergy());
            else
                AddEnergy(energySource.DrainEnergy(Settings_MaxEnergy() - energy));
        }

        if (!isPreviewRobot && SalvageOrReprogramCheck())
            return;

        int energyAfterExecuting = energy - currentInstruction.Setting_EnergyCost();
        
        if (energyAfterExecuting < 0)
            SetFeedback("NOT ENOUGH ENERGY", false, true);
        else if (energySource != null && currentInstructionIndex == 0 && energy != Settings_MaxEnergy())
            SetFeedback("RECHARGING ENERGY", false, true);
        else
        {
            if (ExecuteInstruction(currentInstruction))
                InstructionCompleted();
            energy = energyAfterExecuting;
        }
    }

    [Server]
    private bool SalvageOrReprogramCheck()
    {
        if (IsAtPlayerCity())
        {
            if (willSalvageWhenHome)
            {
                SalvageRobot();
                return true;
            }
            else if (willReprogramWhenHome)
            {
                ReprogramRobot();
                return true;
            }
        }

        return false;
    }

    private bool ExecuteInstruction(Instruction instruction)
    {
        LastAppliedInstruction = instruction;

        if (isPreviewRobot && !instruction.CanBePreviewed())
            return true;
        else if (instruction.GetType() == typeof(Instruction_Unknown))
        {
            SetFeedback(string.Format("UNKNOWN INSTRUCTION: '{0}'", instruction.Serialize()), false, false);
            return true;
        }
        else if (!_allowedInstructions.Any(a => a.GetType() == instruction.GetType()))
        {
            SetFeedback(string.Format("INSTRUCTION NOT ALLOWED: '{0}'", instruction.Serialize()), false, false);
            return true;
        }
        else
            return instruction.Execute(this);
    }

    public void SetFeedback(string message, bool popup, bool whatToSetIsCurrentInstructionValidTo)
    {
        feedback = message;
        currentInstructionIndexIsValid = whatToSetIsCurrentInstructionValidTo;

        if (isPreviewRobot || Owner == null)
            return;

        /* If the feedback has not changed after 1 second we will clear it using a coroutine. */
        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = ClearFeedbackAfterSecondsIfNotChanged(message, 1f);
        StartCoroutine(feedbackClearCoroutine);

        if (popup)
            Owner.ShowPopupForOwner(message, transform.position, TextPopup.ColorType.NEGATIVE);
    }

    private IEnumerator ClearFeedbackAfterSecondsIfNotChanged(string lastFeedback, float secondsDelay)
    {
        yield return new WaitForSeconds(secondsDelay);

        /* We will only clear the feedback if it still the same. Trying to avoid clearing a new feedback set from somewhere else. */
        if (feedback == lastFeedback)
            feedback = string.Empty;
    }

    public bool IsAtPlayerCity()
    {
        if (Owner == null || Owner.City == null)
            return false;
        else
            return x == Owner.City.X && z == Owner.City.Z;
    }

    private bool IsHomeByTransform()
    {
        return transform.position.x == Owner.City.X && transform.position.z == Owner.City.Z;
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

    public void ResetAllInnerLoopStarts(int startingIndex)
    {
        int loopEndSkippingUntilDone = 0;
        for (int i = startingIndex; i < instructions.Count; i++)
        {
            if (Instructions[i].GetType() == typeof(Instruction_LoopStart))
            {
                Instruction_LoopStart loopStartInstruction = (Instruction_LoopStart)Instructions[i];
                loopEndSkippingUntilDone++;
                loopStartInstruction.ResetCurrentIterations();
            }
            else if (Instructions[i].GetType() == typeof(Instruction_LoopEnd))
            {
                if (loopEndSkippingUntilDone == 0)
                    return;
                else
                    loopEndSkippingUntilDone--;
            }
        }
    }

    /// <summary>
    /// Returns true if the item was added successfully
    /// </summary>
    [Server]
    public bool TransferToInventory(InventoryItem item)
    {
        List<InventoryItem> notAddedItems = AddToInventory(new List<InventoryItem> { item });
        return notAddedItems.Count == 0;
    }

    [Server]
    public List<InventoryItem> AddToInventory(List<InventoryItem> items)
    {
        List<InventoryItem> notAdded = new List<InventoryItem>();
        foreach (InventoryItem item in items)
        {
            if (IsInventoryFull())
                notAdded.Add(item);
            else
                inventory.Add(item);
        }

        OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.Serialize(inventory));

        return notAdded;
    }

    [Server]
    public List<InventoryItem> PickUp(int count)
    {
        return inventory.PopLast(count);
    }

    [Server]
    public void SetInventory(List<InventoryItem> items)
    {
        inventory = items;
        OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.Serialize(inventory));
    }

    public bool IsInventoryFull()
    {
        return inventory.Count >= Settings_InventoryCapacity();
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] serializedItemCounts)
    {
        inventory = InventoryItem.Deserialize(serializedItemCounts);
        OnInventoryChanged(this);
    }

    [Command]
    public void CmdAddModule(string serializedModule)
    {
        if (NoFreeModuleSlot())
        {
            SetFeedback("NO FREE MODULE SLOT", true, true);
            return;
        }

        Module module = Module.Deserialize(serializedModule);

        if (Owner.City.CanAfford(module.Settings_Cost()))
        {
            Owner.City.RemoveResources(module.Settings_Cost());
            AddModule(module);
        }
        else
        {
            SetFeedback("NOT ENOUGH RESOURCES FOR MODULE", true, true);
            return;
        }
    }

    [Server]
    public void AddModule(Module module)
    {
        modules.Add(module);
        module.Install(this);
        RpcSyncModules(Module.SerializeList(modules));
    }

    private bool NoFreeModuleSlot()
    {
        return modules.Count >= Settings_ModuleCapacity();
    }

    [ClientRpc]
    private void RpcSyncModules(string[] serializedModules)
    {
        modules = Module.DeserializeList(serializedModules); // Not installed on client.

        if (OnModulesChanged != null)
            OnModulesChanged(this);
    }

    private void SetMaterialColor()
    {
        if (Owner == null) //Neutral
            return;

        if (string.IsNullOrEmpty(Owner.hexColor))
            return;

        if (colorRenderers.Length == 0)
        {
            Debug.LogError(Settings_Name() + " has no team color renderers. Won't be able to indicate team color. Set the rendere object to a GO that will be colored.");
            return;
        }

        var color = Utils.HexToColor(Owner.hexColor);
        foreach (Renderer renderer in colorRenderers)
            renderer.material.color = color;

        isColorSet = true;
    }

    public void AddEnergy(int change)
    {
        if (change < 0)
            throw new Exception("Tried to add negative energy: " + change);

        energy = Math.Min(Settings_MaxEnergy(), energy + change);
    }

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.LogFormat("Robot {0} took {1} damage and now has {2} health", name, damage, health);

        if (health <= 0)
        {
            if (Owner != null)
                Owner.ShowPopupForOwner("DESTROYED!", transform.position, TextPopup.ColorType.NEGATIVE);
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public bool Targetable()
    {
        return !IsAtPlayerCity();
    }

    [Server]
    private void SalvageRobot()
    {
        List<InventoryItem> salvagedResources = new List<InventoryItem>();

        Cost cost = Settings_Cost();
        int salvagedCopper = MathUtils.RoundMin1IfHasValue(cost.Copper * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedCopper; c++)
            salvagedResources.Add(new CopperItem());

        int salvagedIron = MathUtils.RoundMin1IfHasValue(cost.Iron * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedIron; c++)
            salvagedResources.Add(new IronItem());

        Owner.City.AddToInventory(salvagedResources);

        Owner.ShowPopupForOwner("SALVAGED!", transform.position, TextPopup.ColorType.DEFAULT);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ReprogramRobot()
    {
        if (!isReprogrammingRobot)
            StartReprogrammingRobot();

        if (isReprogrammingRobot)
            ContinueReprogrammingRobot();
    }

    [Server]
    private void StartReprogrammingRobot()
    {
        isReprogrammingRobot = true;
        currentInstructionBeingCleared = 1;

        Owner.ShowPopupForOwner("REPROGRAMMNIG!", transform.position, TextPopup.ColorType.DEFAULT);
    }

    [Server]
    private void ContinueReprogrammingRobot()
    {
        currentInstructionClearTickCounter++;

        if (currentInstructionClearTickCounter == Settings.Robot_ReprogramClearEachInstructionTicks)
        {
            currentInstructionBeingCleared++;
            currentInstructionClearTickCounter = 0;
        }

        string feedback = string.Format("Clearing memory {0}/{1}", currentInstructionBeingCleared, instructions.Count);
        for (int i = 0; i < currentInstructionClearTickCounter; i++)
            feedback += ".";

        if (currentInstructionBeingCleared == instructions.Count + 1) // Done reprogramming
        {
            willReprogramWhenHome = false;
            isReprogrammingRobot = false;

            currentInstructionIndex = 0;
            currentInstructionIndexIsValid = true;
            mainLoopIterationCount = 0;
            SetFeedback("", false, true);

            modules.ForEach(module => module.Uninstall(false));
            modules.Clear();
            CacheAllowedInstructions();
            StopRobot();

            Owner.ShowPopupForOwner("MEMORY CLEARED!", transform.position, TextPopup.ColorType.DEFAULT);
        }
        else
            SetFeedback(feedback, false, true);
    }

    public void PreviewReset()
    {
        InitDefaultValues();
        x = transform.position.x;
        z = transform.position.z;
    }
}
