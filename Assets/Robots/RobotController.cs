using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RobotController : NetworkBehaviour, IAttackable, ISelectable, IHasInventory
{
    // ********** COMMON VARIABLES **********
    [SyncVar(hook = "OnOwnerChanged")]
    public string owner = "";

    private PlayerCityController playerCityController;
    public PlayerCityController PlayerCityController { get { return playerCityController; } }

    public GameObject meshGO;

    public Renderer[] colorRenderers;
    private bool isColorSet = false;

    [SyncVar]
    public float x;
    [SyncVar]
    public float z;

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

    [SyncVar]
    protected int health;
    public int Health { get { return health; } }

    // ********** SETTINGS **********

    public abstract string Settings_Name();
    public abstract int Settings_CopperCost();
    public abstract int Settings_IronCost();
    public abstract int Settings_Memory();
    public abstract int Settings_IPT(); // Instructions Per Tick. Cant call it speed because it can be confused with move speed.
    public abstract int Settings_MaxEnergy();
    public abstract int Settings_InventoryCapacity();
    public abstract int Settings_ModuleCapacity();
    public abstract int Settings_HarvestYield();
    public abstract int Settings_Damage();
    public abstract int Settings_StartHealth();
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
        FindPlayerCityController();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isColorSet)
            SetColor();

        if (playerCityController != null)
        {
            EnterExitGarageCheck();
        }
        else
            Debug.LogError(string.Format("{0} owned by {1} does not have playerCityController. NetId: {2}", Settings_Name(), string.IsNullOrEmpty(GetOwner()) ? "MISSING OWNER" : GetOwner(), netId));

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
            playerCityController.EnterGarage(this);
        }
        else if (!IsHomeByTransform() && isAlreadyHome)
        {
            isAlreadyHome = false;
            meshGO.SetActive(true);
            playerCityController.ExitGarage(this);
        }
        else if (IsHomeByTransform() && isAlreadyHome && MouseManager.currentlySelected == gameObject && !isStarted)
            meshGO.SetActive(true);
        else if (IsHomeByTransform() && isAlreadyHome && MouseManager.currentlySelected != this && !isStarted)
            meshGO.SetActive(false);
    }

    [Client]
    private void FindPlayerCityController()
    {
        playerCityController = FindObjectsOfType<PlayerCityController>().FirstOrDefault(x => x.GetOwner() == GetOwner());
        if (playerCityController == null)
            Debug.LogError(Settings_Name() + " did not find it's own PlayerCityController. Please ensure that owner is set before spawning object. NetId: " + netId);
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

    private void OnOwnerChanged(string newValue)
    {
        owner = newValue;
        FindPlayerCityController();
    }

    private void OnIsStartedChanged(bool newValue)
    {
        isStarted = newValue;
        RobotPanel.instance.Refresh(this);
    }

    [Client]
    protected bool ShouldAnimationBePlayed()
    {
        return (energy > 0) && IsStarted && CurrentInstructionIndexIsValid && !isReprogrammingRobot && LastAppliedInstruction != null;
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

            SetInstructions(newInstructions);

            if (instructions.Count <= 0)
            {
                CmdSetFeedback("NO INSTRUCTIONS DETECTED");
                return;
            }

            CmdSetInstructions(InstructionsHelper.SerializeList(instructions));
            CmdStartRobot();

            //For quicker response when changing from setup mode to running mode in GUI. Will be overridden by server when syncvar is synced.
            isStarted = true;
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
    /// Is not set to server because it is used by the preview
    /// </summary>
    public void Tick(object sender)
    {
        SetFeedbackIfNotPreview("");

        if (instructions.Count == 0)
        {
            SetFeedbackIfNotPreview("NO INSTRUCTIONS");
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
            SetFeedbackIfNotPreview("NOT ENOUGH ENERGY");
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
            SetFeedbackIfNotPreview(string.Format("UNKNOWN INSTRUCTION: '{0}'", instruction.Serialize()));
            return true;
        }
        else if (!_allowedInstructions.Any(a => a.GetType() == instruction.GetType()))
        {
            SetFeedbackIfNotPreview(string.Format("INSTRUCTION NOT ALLOWED: '{0}'", instruction.Serialize()));
            return true;
        }
        else
            return instruction.Execute(this);
    }

    public void SetFeedbackIfNotPreview(string message, bool setIsCurrentInstructionIndexValid = false)
    {
        if (isPreviewRobot)
            return;

        _SetFeedback(message, setIsCurrentInstructionIndexValid);

        /* If the feedback has not changed after 1 second we will clear it using a coroutine. */
        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = ClearFeedbackAfterSecondsIfNotChanged(message, 1f);
        StartCoroutine(feedbackClearCoroutine);
    }

    /// <summary>
    /// Never run this method directly, always use SetFeedbackIfNotPreview
    /// </summary>
    [Server]
    private void _SetFeedback(string message, bool setIsCurrentInstructionIndexValid)
    {
        feedback = message;
        currentInstructionIndexIsValid = setIsCurrentInstructionIndexValid;
    }

    [Command]
    private void CmdSetFeedback(string message)
    {
        SetFeedbackIfNotPreview(message);
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
        return x == playerCityController.X && z == playerCityController.Z;
    }

    private bool IsHomeByTransform()
    {
        return transform.position.x == playerCityController.X && transform.position.z == playerCityController.Z;
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
            SetFeedbackIfNotPreview("NO FREE MODULE SLOT", true);
            return;
        }

        Module module = Module.Deserialize(serializedModule);

        bool canAfford = true;
        if (playerCityController.GetCopperCount() < module.Settings_CopperCost())
        {
            playerCityController.FlashMissingResourceForOwner(ResourcePanel.ResourceTypes.Copper);
            canAfford = false;
        }
        if (playerCityController.GetIronCount() < module.Settings_IronCost())
        {
            playerCityController.FlashMissingResourceForOwner(ResourcePanel.ResourceTypes.Iron);
            canAfford = false;
        }

        if (canAfford)
        {
            playerCityController.RemoveResources(module.Settings_CopperCost(), module.Settings_IronCost());
            AddModule(module);
        }
        else
        {
            SetFeedbackIfNotPreview("NOT ENOUGH RESOURCES FOR MODULE", true);
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
        if (playerCityController == null) //Neutral
            return;

        if (string.IsNullOrEmpty(playerCityController.hexColor))
            return;

        if (colorRenderers.Length == 0)
        {
            Debug.LogError(Settings_Name() + " has no team color renderers. Won't be able to indicate team color.");
            return;
        }

        var color = Utils.HexToColor(playerCityController.hexColor);
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

    public T FindOnCurrentPosition<T>()
    {
        return FindNearbyCollidingGameObjects<T>()
            .Where(go => go.transform.position.x == x && go.transform.position.z == z)
            .Select(go => go.transform.root.GetComponent<T>())
            .FirstOrDefault();
    }

    public List<GameObject> FindNearbyCollidingGameObjects<T>(float radius = 7.0f)
    {
        return FindNearbyCollidingGameObjects(radius).Where(go => go.transform.root.GetComponent<T>() != null).ToList();
    }

    public List<GameObject> FindNearbyCollidingGameObjects(float radius = 7.0f)
    {
        return Physics.OverlapSphere(transform.position, radius)
             .Except(new[] { GetComponent<Collider>() })                // Should check if its not the same collider as current collider, not sure if it works
             .Where(c => c.transform.root.gameObject != gameObject)     // Check that it is not the same object
             .Select(c => c.gameObject)
             .ToList();
    }

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.LogFormat("Robot {0} took {1} damage and now has {2} health", name, damage, health);

        if (health <= 0)
        {
            if (playerCityController != null)
                playerCityController.ShowPopupForOwner("DESTROYED!", transform.position, Utils.HexToColor(TextPopup.ColorTypes.NEGATIVE));
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    private void SalvageRobot()
    {
        PlayerCityController playerCity = FindOnCurrentPosition<PlayerCityController>(); //TODO: Safe to replace with playerCityController reference?

        List<InventoryItem> salvagedResources = new List<InventoryItem>();

        int salvagedCopper = MathUtils.RoundMin1IfHasValue(Settings_CopperCost() * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedCopper; c++)
            salvagedResources.Add(new CopperItem());

        int salvagedIron = MathUtils.RoundMin1IfHasValue(Settings_IronCost() * Settings.Robot_SalvagePercentage / 100.0);
        for (int c = 0; c < salvagedIron; c++)
            salvagedResources.Add(new IronItem());

        playerCity.TransferToInventory(salvagedResources);

        playerCityController.ShowPopupForOwner("SALVAGED!", transform.position, Utils.HexToColor(TextPopup.ColorTypes.DEFAULT));

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

        playerCityController.ShowPopupForOwner("REPROGRAMMNIG!", transform.position, Utils.HexToColor(TextPopup.ColorTypes.DEFAULT));
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
            SetFeedbackIfNotPreview("", true);

            modules.ForEach(module => module.Uninstall(false));
            modules.Clear();
            CacheAllowedInstructions();

            playerCityController.ShowPopupForOwner("MEMORY CLEARED!", transform.position, Utils.HexToColor(TextPopup.ColorTypes.DEFAULT));
        }
        else
            SetFeedbackIfNotPreview(feedback, true);
    }

    public string GetOwner()
    {
        return owner;
    }

    public void SetOwner(string owner)
    {
        this.owner = owner;
    }

    public void PreviewResetRobot()
    {
        FindPlayerCityController();

        x = transform.position.x;
        z = transform.position.z;
        transform.position = new Vector3(x, 1, z);
        energy = Settings_MaxEnergy();
        currentInstructionIndex = 0;
        nextInstructionIndex = 0;
        mainLoopIterationCount = 0;
    }
}
