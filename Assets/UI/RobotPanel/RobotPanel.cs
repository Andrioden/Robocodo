using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.GameLogic.ClassExtensions;
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
    public Button closeButton;

    public Button reprogramButton;
    private Image reprogramButtonImage;

    public Button salvageButton;
    private Image salvageButtonImage;

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

    public GameObject arrowPrefab;

    public static RobotPanel instance;

    private Animator animator;
    private RobotController robot;

    public static readonly string _indentation = "   ";
    private List<string> _indentedInstructionsCache = new List<string>();
    private List<string> _formattedInstructions = new List<string>();
    private int _codeInputCharCountLastEdit = 0;
    private int lastCaretPosition = 0;

    private RobotMovementPreviewer previewer;
    private List<GameObject> previewArrows = new List<GameObject>();
    private float drawPreviewArrowTime = -1f;

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
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        RobotController.OnInventoryChanged += InventoryUpdated;

        _highlightColor = Utils.HexToColor("#D5A042FF");
        _defaultButtonStateColors = runButton.GetComponent<Image>().color;
    }

    private void Update()
    {
        if (robot != null)
        {
            if (MouseManager.currentlySelected == null || MouseManager.currentlySelected != robot.gameObject)
            {
                Close();
                return;
            }
            _hadRobotLastFrame = true;

            _formattedInstructions = _indentedInstructionsCache.Select(instruction => instruction.ToString()).ToList();
            UpdateCodeRunningUI();
            UpdateRobotInfoLabels();
            UpdateReprogramAndSalvageButtonsState();

            if (drawPreviewArrowTime != -1f && drawPreviewArrowTime < Time.time)
                DrawPreviewArrows();

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
        this.robot = robot;
        this.robot.GetInstructions().Callback += RobotInstructionsWasUpdated;

        KeyboardManager.KeyboardLockOff();

        if (robot.IsStarted)
            EnableRunningModePanel();
        else
            EnableSetupModePanel();

        animator.Play("RobotMenuSlideIn");
    }

    public void Close()
    {
        KeyboardManager.KeyboardLockOff();
        if (robot != null)
        {
            robot.GetInstructions().Callback -= RobotInstructionsWasUpdated;
            robot.SetInstructions(GetCleanedCodeInput());
        }
        robot = null;
        animator.Play("RobotMenuSlideOut");
        CleanUpPreviewer();
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

    private void RunCode()
    {
        KeyboardManager.KeyboardLockOff();
        robot.RunCode(GetCleanedCodeInput());

        if (robot.IsStarted)
            EnableRunningModePanel();

        CleanUpPreviewer();
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

        if (previewer != null)
        {
            previewer.UpdateInstructions(instructions);
            DrawPreviewArrowsIfNoNewInput();
        }
    }

    private static void AutoIndentInstructions(string indentation, List<string> instructions)
    {
        int currentIndentionLevel = 0;
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].Contains(Instructions.LoopEnd) && currentIndentionLevel > 0)
                currentIndentionLevel--;

            instructions[i] = Utils.RepeatString(indentation, currentIndentionLevel) + instructions[i];

            if (instructions[i].Contains(Instructions.LoopStart))
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

        if (caretTextLine.Trim() == Instructions.LoopEnd)
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

    private void InventoryUpdated(RobotController robot)
    {
        if (this.robot != null && robot == this.robot)
        {
            inventoryContainer.transform.DestroyChildren();

            var currentInventory = this.robot.Inventory;
            currentInventory.ForEach(item => AddInventoryItem(item));
        }
    }

    private void AddInventoryItem(InventoryItem item)
    {
        GameObject prefab = null;
        if (item is IronItem)
            prefab = ironPrefab;
        else if (item is CopperItem)
            prefab = copperPrefab;

        var inventoryImage = Instantiate(prefab) as GameObject;
        inventoryImage.transform.SetParent(inventoryContainer.transform, false);
    }

    private void RobotInstructionsWasUpdated(SyncList<string>.Operation op, int itemIndex)
    {
        LoadInstructionsFromRobot();
    }

    private void LoadInstructionsFromRobot()
    {
        _indentedInstructionsCache = robot.GetInstructions().Select(instruction => instruction.ToString()).ToList();
        AutoIndentInstructions(_indentation, _indentedInstructionsCache);
    }

    private void EnableSetupModePanel()
    {
        if (robot.isPreviewRobot)
            return;

        titleText.text = robot.Settings_Name() + " SETUP";
        SetupPossibleInstructions();

        codeInputPanel.SetActive(true);
        codeInputField.onValueChanged.AddListener(KeyboardManager.KeyboardLockOn);
        codeInputField.onValueChanged.AddListener(CodeInputAutoIndentAndUpperCase);
        codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);
        List<string> exampleInstructions = robot.GetInstructions().ToList();
        codeInputField.text = string.Join("\n", exampleInstructions.ToArray());  /* Pre filled example data */

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(RunCode);

        inventoryPanel.SetActive(false);
        codeRunningPanel.SetActive(false);

        previewer = new RobotMovementPreviewer(robot, exampleInstructions);
        DrawPreviewArrows();
    }

    private void EnableRunningModePanel()
    {
        titleText.text = robot.Settings_Name();
        LoadInstructionsFromRobot();
        InventoryUpdated(robot);

        codeRunningPanel.SetActive(true);
        inventoryPanel.SetActive(true);

        reprogramButton.onClick.RemoveAllListeners();
        reprogramButton.onClick.AddListener(ToggleReprogramming);
        salvageButton.onClick.RemoveAllListeners();
        salvageButton.onClick.AddListener(ToggleSalvaging);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);

        codeInputPanel.SetActive(false);
        possibleInstructionsPanel.SetActive(false);
    }

    private void UpdateReprogramAndSalvageButtonsState()
    {
        if (reprogramButtonImage == null)
            reprogramButtonImage = reprogramButton.GetComponent<Image>();

        if (salvageButtonImage == null)
            salvageButtonImage = salvageButton.GetComponent<Image>();

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
        var combinedList = robot.CommonInstructions.ToList();
        combinedList.Add(string.Empty);
        combinedList.Add(string.Empty);
        combinedList.AddRange(robot.GetSpecializedInstruction());
        possibleInstructionsContainer.transform.DestroyChildren();

        foreach (string instruction in combinedList)
        {
            var possibleInstructionGO = Instantiate(possibleInstructionsPrefab) as GameObject;
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

    private void UpdateCodeRunningUI()
    {
        if (robot.IsStarted)
        {
            if (_formattedInstructions.Count <= 0)
                return;

            if (robot.CurrentInstructionIndexIsValid)
                _formattedInstructions[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, _highlightColor, _formattedInstructions[robot.CurrentInstructionIndex]);
            else
            {
                _formattedInstructions[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, Color.red, _formattedInstructions[robot.CurrentInstructionIndex]);
            }

            codeRunningField.text = string.Join("\n", _formattedInstructions.ToArray());

            if (robot.MainLoopIterationCount == 1)
                codeRunningLabel.text = string.Format("RUNNING CODE ({0} iteration)", robot.MainLoopIterationCount);
            else
                codeRunningLabel.text = string.Format("RUNNING CODE ({0} iterations)", robot.MainLoopIterationCount);
        }
    }

    private void UpdateRobotInfoLabels()
    {
        //If robot has no instructions at this time, get instructions from CodeInputField.
        _formattedInstructions = _formattedInstructions.Count > 0 ? _formattedInstructions : codeInputField.text.Split('\n').ToList();

        inventoryLabel.text = "INVENTORY (" + robot.Inventory.Count + "/" + robot.Settings_InventoryCapacity() + ")";
        memoryText.text = ColorTextOnCondition(_formattedInstructions.Count > robot.Settings_Memory(), Color.red, string.Format("MEMORY: {0}/{1}", _formattedInstructions.Count, robot.Settings_Memory()));
        iptText.text = string.Format("IPT: {0}", robot.Settings_IPT());
        inventoryCapacityText.text = string.Format("INVENTORY CAPACITY: {0}", robot.Settings_InventoryCapacity());
        harvestYieldText.text = string.Format("HARVEST YIELD: {0}", robot.Settings_HarvestYield());
        healthText.text = string.Format("HEALTH: {0}/{1}", robot.Health, robot.Settings_StartHealth());
        damageText.text = string.Format("DAMAGE: {0}", robot.Settings_Damage());
        energyText.text = ColorTextOnCondition(robot.Energy < 4, Color.red, string.Format("ENERGY: {0}/{1}", robot.Energy, robot.Settings_MaxEnergy()));
    }

    private void CleanUpPreviewer()
    {
        DestroyPreviewArrows();

        if (previewer != null)
        {
            previewer.Destroy();
            previewer = null;
        }
    }

    private void DrawPreviewArrowsIfNoNewInput()
    {
        drawPreviewArrowTime = Time.time + 0.3f;
    }

    private void DrawPreviewArrows()
    {
        DestroyPreviewArrows();

        foreach (CoordinateDirection coordDir in previewer.GetPreviewCoordinateDirections())
        {
            GameObject arrowGO = Instantiate(arrowPrefab);
            arrowGO.transform.position = new Vector3(coordDir.x, arrowGO.transform.position.y, coordDir.z);
            Vector3 rotation = arrowGO.transform.rotation.eulerAngles;
            if (coordDir.direction == Direction.Right)
                arrowGO.transform.rotation = Quaternion.Euler(rotation.x, 0, rotation.z);
            else if (coordDir.direction == Direction.Down)
                arrowGO.transform.rotation = Quaternion.Euler(rotation.x, 90, rotation.z);
            else if (coordDir.direction == Direction.Left)
                arrowGO.transform.rotation = Quaternion.Euler(rotation.x, 180, rotation.z);
            else if (coordDir.direction == Direction.Up)
                arrowGO.transform.rotation = Quaternion.Euler(rotation.x, 270, rotation.z);

            previewArrows.Add(arrowGO);
        }

        drawPreviewArrowTime = -1f;
    }

    private void DestroyPreviewArrows()
    {
        foreach (GameObject arrow in previewArrows)
            Destroy(arrow);
    }

    /// <summary>
    /// Has to have a middlemethod run the robot method, cant run the robot.Cmd.. method directly 
    /// </summary>
    public void ToggleReprogramming()
    {
        robot.CmdToggleReprogramWhenHome();
    }

    /// <summary>
    /// Has to have a middlemethod run the robot method, cant run the robot.Cmd.. method directly 
    /// </summary>
    public void ToggleSalvaging()
    {
        robot.CmdToggleSalvageWhenHome();
    }
}
