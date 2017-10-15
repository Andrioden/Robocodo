using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;

public class RobotPanel : MonoBehaviour
{
    public Text titleText;

    public Text feedbackText;

    public Text memoryText;
    public Text iptText;
    public Text inventoryCapacityText;
    public Text harvestYieldText;
    public Text healthText;
    public Text damageText;
    public Text energyText;

    public Button runButton;
    public Button moduleMenuButton;
    public Button closeButton;

    public Button reprogramButton;
    private Image reprogramButtonImage;
    private Text reprogramButtonText;

    public Button salvageButton;
    private Image salvageButtonImage;

    public Button clearCodeButton;
    public Button copyCodeButton;
    public Button pasteCodeButton;

    public Text codeInputLabel;
    public InputField codeInputField;
    public GameObject codeInputPanel;

    public Text codeRunningLabel;
    public Text codeRunningField;
    public GameObject codeRunningPanel;

    public Text possibleInstructionsLabel;
    public GameObject possibleInstructionsContainer;
    public GameObject possibleInstructionsPrefab;
    public GameObject possibleInstructionsPanel;

    public Text inventoryLabel;
    public GameObject inventoryPanel;
    public GameObject inventoryContainer;
    public GameObject ironPrefab;
    public GameObject copperPrefab;
    public GameObject foodPrefab;

    public Text installedModulesLabel;
    public GameObject installedModulesPanel;
    public Text installedModulesListTextField;

    public ModuleMenuController moduleMenuController;

    public static RobotPanel instance;

    private Animator animator;
    private RobotController robot;

    public static readonly string _indentation = "   ";
    private List<string> _indentedInstructionsCache = new List<string>();
    private int _codeInputCharCountLastEdit = 0;
    private int lastCaretPosition = 0;

    private RobotInstructionPreviewer previewer;

    private Color _highlightColor;
    private Color _defaultButtonStateColors;

    private bool _hadRobotLastFrame = false;

    private bool applicationIsQuitting = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        previewer = GetComponent<RobotInstructionPreviewer>();

        _highlightColor = Utils.HexToColor("#D5A042FF");
        _defaultButtonStateColors = runButton.GetComponent<Image>().color;

        if (reprogramButtonImage == null)
            reprogramButtonImage = reprogramButton.GetComponent<Image>();
        if (reprogramButtonText == null)
            reprogramButtonText = reprogramButton.GetComponentInChildren<Text>();

        if (salvageButtonImage == null)
            salvageButtonImage = salvageButton.GetComponent<Image>();
    }

    private void Update()
    {
        if (robot != null)
        {
            if (MouseManager.instance.CurrentlySelectedObject == null || MouseManager.instance.CurrentlySelectedObject != robot.gameObject)
            {
                Close();
                return;
            }
            _hadRobotLastFrame = true;

            UpdateRobotInfoLabels();
            UpdateReprogramAndSalvageButtonsState();

            feedbackText.text = ColorTextOnCondition(true, Color.red, robot.Feedback);
        }
        else if (_hadRobotLastFrame)
        {
            _hadRobotLastFrame = false;
            Close();
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    public void Show(RobotController robot)
    {
        UnloadCurrentRobot();
        LoadRobot(robot);

        KeyboardManager.KeyboardLockOff();

        if (robot.IsStarted)
            EnableRunningModePanel();
        else
            EnableSetupModePanel();

        animator.Play("RobotMenuSlideIn");
        //RTSCamera.instance.PositionRelativeTo(robot.transform, 3);
    }

    public void Close()
    {
        KeyboardManager.KeyboardLockOff();
        UnloadCurrentRobot();
        moduleMenuController.gameObject.SetActive(false);
        animator.Play("RobotMenuSlideOut");
    }

    public void Refresh(RobotController robot)
    {
        if (this.robot != null && robot == this.robot && !applicationIsQuitting)
        {
            KeyboardManager.KeyboardLockOff();

            if (robot.IsStarted)
                EnableRunningModePanel();
            else
                EnableSetupModePanel();
        }
    }

    private void ClearCode()
    {
        codeInputField.text = string.Empty;
    }

    public void CopyCode()
    {
        if (robot.IsStarted)
            GUIUtility.systemCopyBuffer = string.Join("\n", InstructionsHelper.SerializeList(robot.Instructions));
        else
            GUIUtility.systemCopyBuffer = codeInputField.text;
    }

    public void PasteCode()
    {
        codeInputField.text = GUIUtility.systemCopyBuffer;
    }

    private void LoadRobot(RobotController robot)
    {
        this.robot = robot;
        this.robot.OnInstructionsChanged += LoadInstructions;
        this.robot.OnCurrentInstructionIndexChanged += LoadCodeRunning;
        this.robot.OnInventoryChanged += LoadInventory;
        this.robot.OnModulesChanged += LoadModules;
    }

    private void UnloadCurrentRobot()
    {
        if (robot != null)
        {
            robot.OnInstructionsChanged -= LoadInstructions;
            robot.OnCurrentInstructionIndexChanged -= LoadCodeRunning;
            robot.OnInventoryChanged -= LoadInventory;
            robot.OnModulesChanged -= LoadModules;
            if (!robot.IsStarted)
                robot.SetInstructions(GetCleanedCodeInput());

            robot = null;

            previewer.Unload();
        }
    }

    private void RunRobot()
    {
        KeyboardManager.KeyboardLockOff();
        robot.Run(GetCleanedCodeInput());

        if (robot.IsStarted)
            EnableRunningModePanel();
    }

    private List<string> GetCleanedCodeInput()
    {
        return codeInputField.text.Split('\n').Where(x => x.Trim() != string.Empty).Select(x => x.Trim()).ToList();
    }

    public void AppendNewInstructionAtCaretPosition(string instructionString)
    {
        if (string.IsNullOrEmpty(instructionString))
            throw new Exception("Tried to append empty instruction string to code input.");

        int lastKnowCaretPosition = RobotPanel.instance.GetLastKnownCaretPosition();
        if (lastKnowCaretPosition > 0)
        {
            string caretTextLine = RobotPanel.instance.GetCarretTextLineWithoutLineBreaks(lastKnowCaretPosition - 1);
            instructionString = caretTextLine == string.Empty ? instructionString : "\n" + instructionString;
            codeInputField.text = codeInputField.text.Insert(GetEndIndexOfCaretLine(lastKnowCaretPosition), instructionString);
            codeInputField.caretPosition = lastKnowCaretPosition + instructionString.Length; //Won't be end of line if indented. Get end of line when inserting next instruction.
        }
        else
        {
            instructionString = codeInputField.text == string.Empty ? instructionString : "\n" + instructionString;
            codeInputField.text += instructionString;
        }

        SaveCaretPositions();
    }

    private void CodeInputAutoIndentAndUpperCase(string arg0 = "")
    {
        arg0 = arg0.ToUpper();
        List<string> instructions = arg0.Split('\n').Select(x => x.Replace(_indentation, "")).ToList();
        AutoIndentInstructions(_indentation, instructions);

        /* Disable onValueChanged listener to avoid loop when changing input field value */
        codeInputField.onValueChanged.RemoveListener(CodeInputAutoIndentAndUpperCase);
        codeInputField.text = string.Join("\n", instructions.ToArray());
        codeInputField.onValueChanged.AddListener(CodeInputAutoIndentAndUpperCase);

        bool changeWasBackspace = _codeInputCharCountLastEdit - arg0.Count() == 1;
        if (!changeWasBackspace)
            AdjustCaretBasedOnContext();
        _codeInputCharCountLastEdit = codeInputField.text.Count();

        previewer.UpdateInstructions(InstructionsHelper.Deserialize(instructions));
        previewer.DrawPreviewAfterDelay(0.1f);

        if (codeInputField.isFocused)
            KeyboardManager.KeyboardLockOn();
    }

    private static void AutoIndentInstructions(string indentation, List<string> instructions)
    {
        int currentIndentionLevel = 0;
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i] == Instruction_LoopEnd.Format && currentIndentionLevel > 0)
                currentIndentionLevel--;

            instructions[i] = Utils.RepeatString(indentation, currentIndentionLevel) + instructions[i];

            if (Instruction_LoopStart.IsValid(instructions[i]))
                currentIndentionLevel++;
        }
    }

    private void AdjustCaretBasedOnContext()
    {
        int caretPosition = GetLastKnownCaretPosition();
        if (caretPosition <= 0) return;

        string caretTextLine = GetCarretTextLineWithoutLineBreaks(caretPosition);
        if (caretTextLine == "") return;

        int numberOfCharactersInCaretTextLine = caretTextLine.Count();
        if (Utils.ConsistsOfWhiteSpace(caretTextLine))
            codeInputField.caretPosition += _indentation.Count() * numberOfCharactersInCaretTextLine / _indentation.Count();

        LoopEndBetweenInstructionsCaretFix(_indentation, caretPosition);
    }

    private void LoopEndBetweenInstructionsCaretFix(string indentation, int caretPosition)
    {
        //Backtracing if we just entered a Loop End between two instructions and moving caret back the appropriate amount of characters to avoid the caret skipping ahead to the middle of next line.
        int identationPlusLineBreakCharacterCount = indentation.Count() + 1;
        int potentialLoopEndCarretPosition = identationPlusLineBreakCharacterCount;
        string caretTextLine = GetCarretTextLineWithoutLineBreaks(caretPosition - potentialLoopEndCarretPosition);

        if (codeInputField.text.IndexOf("\n", caretPosition) == -1)
            return;

        if (caretTextLine.Trim() == Instruction_LoopEnd.Format)
            codeInputField.caretPosition -= indentation.Count();
    }

    public string GetCarretTextLineWithoutLineBreaks(int caretPosition)
    {
        if (caretPosition < codeInputField.text.Length && caretPosition <= 0 || caretPosition > codeInputField.text.Length)
            return "";

        var startOfCaretTextLine = GetStartIndexOfCaretLine(caretPosition);
        var endOfCaretTextLine = GetEndIndexOfCaretLine(caretPosition);

        string caretTextLine = string.Empty;
        if ((startOfCaretTextLine >= 0 && endOfCaretTextLine > 0) && startOfCaretTextLine < endOfCaretTextLine)
        {
            caretTextLine = codeInputField.text.Substring(startOfCaretTextLine, endOfCaretTextLine - startOfCaretTextLine);
            return caretTextLine.Replace("\n", "");
        }

        return "";
    }

    public int GetStartIndexOfCaretLine(int caretPosition)
    {
        if (caretPosition < codeInputField.text.Length && caretPosition <= 0 || caretPosition > codeInputField.text.Length)
            return 0;

        int startOfCaretTextLine = codeInputField.text.LastIndexOf("\n", caretPosition, caretPosition);

        if (startOfCaretTextLine == -1)
            startOfCaretTextLine = 0;

        return startOfCaretTextLine;
    }

    public int GetEndIndexOfCaretLine(int caretPosition)
    {
        if (caretPosition < codeInputField.text.Length && caretPosition <= 0 || caretPosition > codeInputField.text.Length)
            return 0;

        var endOfCaretTextLine = codeInputField.text.IndexOf("\n", caretPosition);

        if (endOfCaretTextLine == -1)
            endOfCaretTextLine = codeInputField.text.Length;

        return endOfCaretTextLine;
    }

    public void SaveCaretPositions()
    {
        if (codeInputField.caretPosition > 0)
            lastCaretPosition = codeInputField.caretPosition;
    }

    public int GetLastKnownCaretPosition()
    {
        return codeInputField.caretPosition == 0 ? lastCaretPosition : codeInputField.caretPosition;
    }

    private void LoadInventory()
    {
        inventoryContainer.transform.DestroyChildren();

        robot.Inventory.ForEach(item => AddInventoryItem(item));
    }

    private void AddInventoryItem(InventoryItem item)
    {
        GameObject prefab = null;
        if (item is IronItem)
            prefab = ironPrefab;
        else if (item is CopperItem)
            prefab = copperPrefab;
        else if (item is FoodItem)
            prefab = foodPrefab;

        var inventoryImage = Instantiate(prefab) as GameObject;
        inventoryImage.transform.SetParent(inventoryContainer.transform, false);
    }

    private void LoadInstructions()
    {
        _indentedInstructionsCache = robot.Instructions.Select(i => i.Serialize()).ToList();
        AutoIndentInstructions(_indentation, _indentedInstructionsCache);
    }

    private void LoadModules()
    {
        installedModulesListTextField.text = string.Join("\n", robot.Modules.Select(m => m.Settings_Name()).ToArray());
    }

    private void EnableSetupModePanel()
    {
        if (robot.isPreviewRobot)
            return;

        titleText.text = robot.Settings_Name();
        SetupPossibleInstructions();
        LoadModules();

        previewer.Load(robot);

        codeInputPanel.SetActive(true);
        codeInputField.text = string.Join("\n", InstructionsHelper.SerializeList(robot.Instructions));  /* Pre filled example data */
        codeInputField.onValueChanged.RemoveAllListeners();
        codeInputField.onValueChanged.AddListener(CodeInputAutoIndentAndUpperCase);
        codeInputField.onEndEdit.RemoveAllListeners();
        codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(RunRobot);

        moduleMenuButton.onClick.RemoveAllListeners();
        moduleMenuButton.onClick.AddListener(ToggleShowModuleMenu);
        moduleMenuButton.GetComponentInChildren<Text>().text = "ADD MODULES";

        SetupClearCopyPasteButtons(true);

        inventoryPanel.SetActive(false);
        codeRunningPanel.SetActive(false);
    }

    private void SetupClearCopyPasteButtons(bool isSetupMode)
    {
        copyCodeButton.onClick.RemoveAllListeners();
        copyCodeButton.onClick.AddListener(CopyCode);

        if (isSetupMode)
        {
            clearCodeButton.gameObject.SetActive(true);
            clearCodeButton.onClick.RemoveAllListeners();
            clearCodeButton.onClick.AddListener(ClearCode);

            pasteCodeButton.gameObject.SetActive(true);
            pasteCodeButton.onClick.RemoveAllListeners();
            pasteCodeButton.onClick.AddListener(PasteCode);
        }
        else
        {
            clearCodeButton.gameObject.SetActive(false);
            pasteCodeButton.gameObject.SetActive(false);
        }
    }

    private void ToggleShowModuleMenu()
    {
        if (moduleMenuController.gameObject.activeSelf)
        {
            moduleMenuController.gameObject.SetActive(false);
            moduleMenuButton.GetComponentInChildren<Text>().text = "ADD MODULES";
        }
        else
        {
            moduleMenuController.Setup(robot);
            moduleMenuController.gameObject.SetActive(true);
            moduleMenuButton.GetComponentInChildren<Text>().text = "CLOSE";
        }
    }

    private void EnableRunningModePanel()
    {
        titleText.text = robot.Settings_Name();
        LoadInstructions();
        LoadCodeRunning(0);
        LoadInventory();
        LoadModules();

        previewer.Load(robot);

        codeRunningPanel.SetActive(true);
        inventoryPanel.SetActive(true);

        reprogramButton.onClick.RemoveAllListeners();
        reprogramButton.onClick.AddListener(ToggleReprogramming);
        reprogramButtonText.text = string.Format("REPROGRAM when home ({0}t)", robot.Instructions.Count * Settings.Robot_ReprogramClearEachInstructionTicks);

        salvageButton.onClick.RemoveAllListeners();
        salvageButton.onClick.AddListener(ToggleSalvaging);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);

        SetupClearCopyPasteButtons(false);

        codeInputPanel.SetActive(false);
        possibleInstructionsPanel.SetActive(false);
    }

    private void UpdateReprogramAndSalvageButtonsState()
    {
        if (robot.WillReprogramWhenHome)
            reprogramButtonImage.color = _highlightColor;
        else
            reprogramButtonImage.color = _defaultButtonStateColors;

        if (robot.WillSalvageWhenHome)
            salvageButtonImage.color = _highlightColor;
        else
            salvageButtonImage.color = _defaultButtonStateColors;
    }

    private string ColorTextOnCondition(bool condition, Color color, string text)
    {
        return ColorTextOnCondition(condition, Utils.ColorToHex(color), text);
    }

    private string ColorTextOnCondition(bool condition, string hexColor, string text)
    {
        if (!hexColor.Contains("#"))
            hexColor = "#" + hexColor;

        if (condition) return string.Format("<color={0}>{1}</color>", hexColor, text);
        else return text;
    }

    private void SetupPossibleInstructions()
    {
        possibleInstructionsPanel.SetActive(true);
        var combinedList = InstructionsHelper.SerializeList(robot.CommonInstructions).ToList();
        combinedList.Add(string.Empty);
        combinedList.Add(string.Empty);
        combinedList.AddRange(InstructionsHelper.SerializeList(robot.GetSpecializedInstructions()));
        possibleInstructionsContainer.transform.DestroyChildren();

        foreach (string instruction in combinedList)
        {
            GameObject possibleInstructionGO = Instantiate(possibleInstructionsPrefab);
            possibleInstructionGO.transform.SetParent(possibleInstructionsContainer.transform, false);

            if (instruction != string.Empty)
            {
                possibleInstructionGO.GetComponent<Text>().text = instruction;
                possibleInstructionGO.GetComponent<PossibleInstructionOnClick>().SetupPossibleInstruction(codeInputField, instruction);
            }
            else
                possibleInstructionGO.GetComponent<PossibleInstructionOnClick>().enabled = false;
        }
    }

    /// <summary>
    /// Input value not used, only given here to listen to event
    /// </summary>
    private void LoadCodeRunning(int _)
    {
        if (robot.IsStarted)
        {
            List<string> instructionsClone = new List<string>(_indentedInstructionsCache);

            if (instructionsClone.Count == 0)
                return;

            if (robot.CurrentInstructionIndexIsValid)
                instructionsClone[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, _highlightColor, instructionsClone[robot.CurrentInstructionIndex]);
            else
            {
                instructionsClone[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, Color.red, instructionsClone[robot.CurrentInstructionIndex]);
            }

            codeRunningField.text = string.Join("\n", instructionsClone.ToArray());

            if (robot.MainLoopIterationCount == 1)
                codeRunningLabel.text = string.Format("RUNNING CODE ({0} iteration)", robot.MainLoopIterationCount);
            else
                codeRunningLabel.text = string.Format("RUNNING CODE ({0} iterations)", robot.MainLoopIterationCount);
        }
    }

    private void UpdateRobotInfoLabels()
    {
        int instructionsCount = codeInputField.text.Split('\n').Length;

        inventoryLabel.text = string.Format("INVENTORY ({0}/{1})", robot.Inventory.Count, robot.Settings_InventoryCapacity());
        installedModulesLabel.text = string.Format("MODULES ({0}/{1})", robot.Modules.Count, robot.Settings_ModuleCapacity());

        memoryText.text = ColorTextOnCondition(instructionsCount > robot.Settings_Memory(), Color.red, string.Format("MEMORY: {0}/{1}", instructionsCount, robot.Settings_Memory()));
        iptText.text = string.Format("IPT: {0}", robot.Settings_IPT());
        inventoryCapacityText.text = string.Format("INVENTORY CAPACITY: {0}", robot.Settings_InventoryCapacity());
        harvestYieldText.text = string.Format("HARVEST YIELD: {0}", robot.Settings_HarvestYield());
        healthText.text = string.Format("HEALTH: {0}/{1}", robot.Health, robot.Settings_StartHealth());
        damageText.text = string.Format("DAMAGE: {0}", robot.Settings_Damage());
        energyText.text = ColorTextOnCondition(robot.Energy < 4, Color.red, string.Format("ENERGY: {0}/{1}", robot.Energy, robot.Settings_MaxEnergy()));
    }

    /// <summary>
    /// Has to have a method in class, cant run the robot.Cmd.. method directly 
    /// </summary>
    private void ToggleReprogramming()
    {
        robot.CmdToggleReprogramWhenHome();
    }

    /// <summary>
    /// Has to have a method in class, cant run the robot.Cmd.. method directly 
    /// </summary>
    private void ToggleSalvaging()
    {
        robot.CmdToggleSalvageWhenHome();
    }
}
