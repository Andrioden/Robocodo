using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RobotController : ActingEntity, IAttackable, IOwned, ISelectable, IHasInventory
{
    // ********** COMMON VARIABLES **********

    private PlayerCityController ownerCity;
    public PlayerCityController OwnerCity { get { return ownerCity; } }

    public GameObject meshGO;

    public Renderer[] colorRenderers;
    private bool isColorSet = false;

    private bool isAlreadyHome = false;

    [SyncVar]
    private string feedback = "";
    public string Feedback { get { return feedback; } }

    private IEnumerator feedbackClearCoroutine;

    [SyncVar(hook = "OnIsStartedChanged")]
    private bool isStarted = false;
    public bool IsStarted { get { return isStarted; } }

    public bool isPreviewRobot = false;

    protected List<Instruction> instructions = new List<Instruction>();
    public List<Instruction> Instructions { get { return instructions; } }
    public delegate void InstructionsChanged(RobotController robot);
    public static event InstructionsChanged OnInstructionsChanged = delegate { };
    public void NotifyInstructionsChanged() { OnInstructionsChanged(this); }

    [SyncVar]
    public int nextInstructionIndex = 0;

    [SyncVar]
    protected int currentInstructionIndex = 0;
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

    private List<Instruction> _allowedInstructions = new List<Instruction>();

    [SyncVar]
    private int mainLoopIterationCount = 0;
    public int MainLoopIterationCount { get { return mainLoopIterationCount; } }

    private List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> Inventory { get { return inventory; } }
    public delegate void InventoryChanged(RobotController robot);
    public static event InventoryChanged OnInventoryChanged = delegate { };

    private List<Module> modules = new List<Module>();
    public List<Module> Modules { get { return modules; } }
    public delegate void ModulesChanged(RobotController robot);
    public static event ModulesChanged OnModulesChanged = delegate { };

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

        InitDefaultValues();

        if (GameObjectUtils.FindClientsOwnPlayerCity() == ownerCity)
            StackingRobotsOverhangManager.instance.RefreshStackingRobotsOverheads();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isColorSet)
            SetColor();

        if (ownerCity != null)
            EnterExitGarageCheck();

        Move();
        Animate();
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            StopRobot();
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
        CacheAllowedInstructions();

        x = transform.position.x;
        z = transform.position.z;

        energy = Settings_MaxEnergy();
        health = Settings_StartHealth();

        if (instructions.Count == 0)
            SetInstructions(GetSuggestedInstructionSet());
    }

    private void EnterExitGarageCheck()
    {
        if (IsHomeByTransform() && !isAlreadyHome)
        {
            isAlreadyHome = true;
            meshGO.SetActive(false);
            ownerCity.EnterGarage(this);
        }
        else if (!IsHomeByTransform() && isAlreadyHome)
        {
            isAlreadyHome = false;
            meshGO.SetActive(true);
            ownerCity.ExitGarage(this);
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
        MovementBasedFacingDirection(newPosition);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * Settings_IPT());
    }

    [Client]
    private void MovementBasedFacingDirection(Vector3 newPosition)
    {
        if (LastAppliedInstruction != null && (LastAppliedInstruction.GetType() == typeof(Instruction_Move)))
            transform.LookAt(newPosition);
    }

    [Client]
    public void NonMovementBasedFacingDirection()
    {
        Vector3? facePosition = null;

        if (instructions.Count > 0 && LastAppliedInstruction.GetType() == typeof(Instruction_Attack))
        {
            Instruction_Attack attackInstruction = (Instruction_Attack)LastAppliedInstruction;

            if (attackInstruction.direction == AttackDirection.Up)
                facePosition = new Vector3(x, transform.position.y, z + 2);
            else if (attackInstruction.direction == AttackDirection.Down)
                facePosition = new Vector3(x, transform.position.y, z - 2);
            else if (attackInstruction.direction == AttackDirection.Left)
                facePosition = new Vector3(x - 2, transform.position.y, z);
            else if (attackInstruction.direction == AttackDirection.Right)
                facePosition = new Vector3(x + 2, transform.position.y, z);
        }

        if (facePosition.HasValue)
            transform.LookAt(facePosition.Value);
    }

    [ClientRpc]
    private void RpcSyncLastAppliedInstruction(string instructionString)
    {
        lastAppliedInstruction = InstructionsHelper.Deserialize(instructionString);

        /* We never want to change facing or animate preview robot */
        if (isPreviewRobot)
            return;

        NonMovementBasedFacingDirection();
    }

    [Client]
    private void OnIsStartedChanged(bool newValue)
    {
        isStarted = newValue;
        RobotPanel.instance.Refresh(this);
    }

    [Client]
    protected bool ShouldAnimationBePlayed()
    {
        return (energy > 0) && IsStarted && currentInstructionIndexIsValid && !isReprogrammingRobot && LastAppliedInstruction != null;
    }

    [Client]
    public void Run(List<string> newInstructions)
    {
        if (hasAuthority && !isStarted)
        {
            if (newInstructions.Count > Settings_Memory())
            {
                SetFeedbackIfNotPreview("NOT ENOUGH MEMORY", true, true);
                return;
            }

            SetInstructions(newInstructions);

            if (instructions.Count <= 0)
            {
                SetFeedbackIfNotPreview("NO INSTRUCTIONS DETECTED", true, true);
                return;
            }

            CmdSetInstructions(InstructionsHelper.SerializeList(instructions));
            CmdStartRobot();

            //For quicker response when changing from setup mode to running mode in GUI. Will be overridden by server when syncvar is synced.
            isStarted = true;
            StackingRobotsOverhangManager.instance.RefreshStackingRobotsOverheads();
        }
    }

    [Command]
    private void CmdSetInstructions(string[] instructionStrings)
    {
        instructions = InstructionsHelper.DeserializeList(instructionStrings.ToList());
    }

    [Client]
    public void SetInstructions(List<Instruction> newInstructions)
    {
        instructions = newInstructions;
    }

    [Client]
    public void SetInstructions(List<string> instructionsList)
    {
        instructions = InstructionsHelper.DeserializeList(instructionsList);
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
        //Debug.Log("Server: Starting robot");
        isStarted = true;
        if (Settings_IPT() == 1)
            WorldTickController.instance.TickEvent += Tick;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.HalfTickEvent += Tick;
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
            WorldTickController.instance.TickEvent -= Tick;
        else if (Settings_IPT() == 2)
            WorldTickController.instance.HalfTickEvent -= Tick;
    }

    /// <summary>
    /// Is not set to [Server] because it is used by the preview
    /// </summary>
    public void Tick(object sender)
    {
        SetFeedbackIfNotPreview("", false, true);

        if (instructions.Count == 0)
        {
            SetFeedbackIfNotPreview("NO INSTRUCTIONS", true, true);
            return;
        }

        currentInstructionIndexIsValid = true;
        currentInstructionIndex = nextInstructionIndex;
        Instruction currentInstruction = instructions[nextInstructionIndex];

        //Debug.Log("SERVER: Running instruction: " + instruction);

        if (IsOnEnergySource())
            AddEnergy(Settings_MaxEnergy());

        if (SalvageOrReprogramCheck())
            return;

        if (energy <= 0)
            SetFeedbackIfNotPreview("NOT ENOUGH ENERGY", true, true);
        else
        {
            if (ExecuteInstruction(currentInstruction))
                InstructionCompleted();
            energy--;
        }
    }

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
            SetFeedbackIfNotPreview(string.Format("UNKNOWN INSTRUCTION: '{0}'", instruction.Serialize()), false, false);
            return true;
        }
        else if (!_allowedInstructions.Any(a => a.GetType() == instruction.GetType()))
        {
            SetFeedbackIfNotPreview(string.Format("INSTRUCTION NOT ALLOWED: '{0}'", instruction.Serialize()), false, false);
            return true;
        }
        else
            return instruction.Execute(this);
    }

    public void SetFeedbackIfNotPreview(string message, bool showPopupInWorld, bool whatToSetIsCurrentInstructionValidTo)
    {
        if (isPreviewRobot)
            return;

        _SetFeedback(message, whatToSetIsCurrentInstructionValidTo);

        /* If the feedback has not changed after 1 second we will clear it using a coroutine. */
        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = ClearFeedbackAfterSecondsIfNotChanged(message, 1f);
        StartCoroutine(feedbackClearCoroutine);

        if (showPopupInWorld)
            ownerCity.ShowPopupForOwner(message, transform.position, TextPopup.ColorType.NEGATIVE);
    }

    private void _SetFeedback(string message, bool setIsCurrentInstructionIndexValid)
    {
        feedback = message;
        currentInstructionIndexIsValid = setIsCurrentInstructionIndexValid;
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
        if (ownerCity == null)
            return false;
        else
            return x == ownerCity.X && z == ownerCity.Z;
    }

    private bool IsHomeByTransform()
    {
        return transform.position.x == ownerCity.X && transform.position.z == ownerCity.Z;
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

        OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.SerializeList(inventory));

        return notAdded;
    }

    [Server]
    public void SetInventory(List<InventoryItem> items)
    {
        inventory = items;
        OnInventoryChanged(this);

        RpcSyncInventory(InventoryItem.SerializeList(inventory));
    }

    public bool IsInventoryFull()
    {
        return inventory.Count >= Settings_InventoryCapacity();
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] serializedItemCounts)
    {
        inventory = InventoryItem.DeserializeList(serializedItemCounts);
        OnInventoryChanged(this);
    }

    [Command]
    public void CmdAddModule(string serializedModule)
    {
        if (NoFreeModuleSlot())
        {
            SetFeedbackIfNotPreview("NO FREE MODULE SLOT", true, true);
            return;
        }

        Module module = Module.Deserialize(serializedModule);

        if (ownerCity.CanAfford(module.Settings_Cost()))
        {
            ownerCity.RemoveResources(module.Settings_Cost());
            AddModule(module);
        }
        else
        {
            SetFeedbackIfNotPreview("NOT ENOUGH RESOURCES FOR MODULE", true, true);
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

    private void SetColor()
    {
        if (ownerCity == null) //Neutral
            return;

        if (string.IsNullOrEmpty(ownerCity.hexColor))
            return;

        if (colorRenderers.Length == 0)
        {
            Debug.LogError(Settings_Name() + " has no team color renderers. Won't be able to indicate team color.");
            return;
        }

        var color = Utils.HexToColor(ownerCity.hexColor);
        foreach (Renderer renderer in colorRenderers)
            renderer.material.color = color;

        isColorSet = true;
    }

    private bool IsOnEnergySource()
    {
        return FindOnCurrentPosition<IEnergySource>() != null;
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
            if (ownerCity != null)
                ownerCity.ShowPopupForOwner("DESTROYED!", transform.position, TextPopup.ColorType.NEGATIVE);
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
        PlayerCityController playerCity = FindOnCurrentPosition<PlayerCityController>(); //TODO: Safe to replace with playerCityController reference?

        List<InventoryItem> salvagedResources = new List<InventoryItem>();

        Cost cost = Settings_Cost();
        int salvagedCopper = MathUtils.RoundMin1IfHasValue(cost.Copper * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedCopper; c++)
            salvagedResources.Add(new CopperItem());

        int salvagedIron = MathUtils.RoundMin1IfHasValue(cost.Iron * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedIron; c++)
            salvagedResources.Add(new IronItem());

        playerCity.TransferToInventory(salvagedResources);

        ownerCity.ShowPopupForOwner("SALVAGED!", transform.position, TextPopup.ColorType.DEFAULT);

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

        ownerCity.ShowPopupForOwner("REPROGRAMMNIG!", transform.position, TextPopup.ColorType.DEFAULT);
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

            StopRobot();
            currentInstructionIndex = 0;
            currentInstructionIndexIsValid = true;
            mainLoopIterationCount = 0;
            SetFeedbackIfNotPreview("", false, true);

            modules.ForEach(module => module.Uninstall(false));
            modules.Clear();
            CacheAllowedInstructions();

            ownerCity.ShowPopupForOwner("MEMORY CLEARED!", transform.position, TextPopup.ColorType.DEFAULT);
        }
        else
            SetFeedbackIfNotPreview(feedback, false, true);
    }

    public PlayerCityController GetOwnerCity()
    {
        return ownerCity;
    }

    [Server]
    public void SetAndSyncOwnerCity(string connectionId)
    {
        ownerCity = WorldController.instance.FindPlayerCityController(connectionId);
        RpcSetOwnerCity(connectionId);
    }

    [ClientRpc]
    private void RpcSetOwnerCity(string connectionId)
    {
        ownerCity = WorldController.instance.FindPlayerCityController(connectionId);
    }

    [TargetRpc]
    public void TargetSetOwnerCity(NetworkConnection target, string connectionId)
    {
        ownerCity = WorldController.instance.FindPlayerCityController(connectionId);
    }

    [Client]
    public void SetOwnerCity(PlayerCityController ownerCity)
    {
        this.ownerCity = ownerCity;
    }

    public void PreviewResetRobot()
    {
        x = transform.position.x;
        z = transform.position.z;
        transform.position = new Vector3(x, 1, z);
        energy = Settings_MaxEnergy();
        currentInstructionIndex = 0;
        nextInstructionIndex = 0;
        mainLoopIterationCount = 0;
    }
}
