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
    public Text energyText;
    public Text healthText;
    public Text damageText;

    public Button runButton;
    public Button closeButton;

    public Text codeInputLabel;
    public InputField codeInputField;
    public GameObject codeInputPanel;

    public Text codeOutputLabel;
    public Text codeOutputField;
    public GameObject codeOutputPanel;

    public Text possibleCommandsLabel;
    public GameObject possibleCommandsContainer;
    public GameObject possibleCommandPrefab;
    public GameObject possibleCommandsPanel;

    public Text inventoryLabel;
    public GameObject inventoryPanel;
    public GameObject inventoryContainer;
    public GameObject ironPrefab;
    public GameObject copperPrefab;

    public GameObject arrowPrefab;

    public static RobotPanel instance;

    private Animator animator;
    private RobotController robot;
    private IEnumerator feedbackClearCoroutine;

    public static readonly string _indentation = "   ";
    private List<string> _indentedInstructionsCache = new List<string>();
    private List<string> _formattedInstructions = new List<string>();
    private int _codeInputCharCountLastEdit = 0;
    private int lastCaretPosition = 0;

    private RobotMovementPreviewer previewer;
    private List<GameObject> previewArrows = new List<GameObject>();
    private float drawPreviewArrowTime = -1f;

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
    }

    private void Update()
    {
        if (robot != null)
        {
            if (MouseManager.currentlySelected == null || MouseManager.currentlySelected != robot.gameObject)
            {
                ClosePanel();
                return;
            }

            _formattedInstructions = _indentedInstructionsCache.Select(instruction => instruction.ToString()).ToList();
            HighlightCode();
            UpdateRobotInfoLabels();

            if (drawPreviewArrowTime != -1f && drawPreviewArrowTime < Time.time)
                DrawPreviewArrows();
        }
    }

    public void ShowPanel(RobotController robot)
    {
        this.robot = robot;
        this.robot.GetInstructions().Callback += RobotInstructionsWasUpdated;

        KeyboardManager.KeyboardLockOff();

        if (robot.IsStarted)
            EnableRunnningModePanel();
        else
            EnableSetupModePanel();

        animator.Play("RobotMenuSlideIn");
    }

    private void RunCode()
    {
        KeyboardManager.KeyboardLockOff();
        List<string> instructions = codeInputField.text.Split('\n').Select(x => x.Trim()).ToList();

        robot.RunCode(instructions);

        if (robot.IsStarted)
            EnableRunnningModePanel();

        CleanUpPreviewer();
    }

    public void ClosePanel()
    {
        KeyboardManager.KeyboardLockOff();
        robot = null;
        animator.Play("RobotMenuSlideOut");
        CleanUpPreviewer();
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
            codeInputField.text = codeInputField.text.Insert(lastKnowCaretPosition, instructionString);
            //TODO: This caret repositioning does not work for loop start. Debug.
            codeInputField.caretPosition = lastKnowCaretPosition + instructionString.Length;
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

        /* Disable onValueChanged listener to avoid loop when chaning input field value */
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
            if (instructions[i].Contains(Instructions.LoopEnd))
                currentIndentionLevel--;

            instructions[i] = Utils.RepeatString(indentation, currentIndentionLevel) + instructions[i];

            if (instructions[i].Contains(Instructions.LoopStart))
                currentIndentionLevel++;
        }
    }

    public void AdjustCaretBasedOnContext()
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

        var startOfCaretTextLine = codeInputField.text.LastIndexOf("\n", caretPosition, caretPosition);
        var endOfCaretTextLine = codeInputField.text.IndexOf("\n", caretPosition);

        if (startOfCaretTextLine == -1)
            startOfCaretTextLine = 0;

        if (endOfCaretTextLine == -1)
            endOfCaretTextLine = codeInputField.text.Length;

        string caretTextLine = string.Empty;
        if ((startOfCaretTextLine >= 0 && endOfCaretTextLine > 0) && startOfCaretTextLine < endOfCaretTextLine)
        {
            caretTextLine = codeInputField.text.Substring(startOfCaretTextLine, endOfCaretTextLine - startOfCaretTextLine);
            return caretTextLine.Replace("\n", "");
        }

        return "";
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

    private void SetFeedbackText(string feedback, float durationSeconds)
    {
        feedbackText.text = feedback.ToUpper();

        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = RemoveFeedbackAfterSeconds(durationSeconds);
        StartCoroutine(feedbackClearCoroutine);
    }

    IEnumerator RemoveFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        feedbackText.text = "";
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

        titleText.text = robot.GetName() + " SETUP";
        SetupPossibleCommands();

        codeInputPanel.SetActive(true);
        codeInputField.onValueChanged.AddListener(KeyboardManager.KeyboardLockOn);
        codeInputField.onValueChanged.AddListener(CodeInputAutoIndentAndUpperCase);
        codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);
        List<string> exampleInstructions = robot.GetExampleInstructions();
        codeInputField.text = string.Join("\n", exampleInstructions.ToArray());  /* Pre filled example data */

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(RunCode);

        inventoryPanel.SetActive(false);
        codeOutputPanel.SetActive(false);

        previewer = new RobotMovementPreviewer(robot, exampleInstructions);
        DrawPreviewArrows();
    }

    private void EnableRunnningModePanel()
    {
        titleText.text = robot.GetName();
        LoadInstructionsFromRobot();
        InventoryUpdated(robot);

        codeOutputPanel.SetActive(true);
        inventoryPanel.SetActive(true);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);

        codeInputPanel.SetActive(false);
        possibleCommandsPanel.SetActive(false);
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

    private void SetupPossibleCommands()
    {
        possibleCommandsPanel.SetActive(true);
        var combinedList = robot.CommonInstructions.ToList();
        combinedList.Add(string.Empty);
        combinedList.Add(string.Empty);
        combinedList.AddRange(robot.GetSpecializedInstruction());
        possibleCommandsContainer.transform.DestroyChildren();

        foreach (string instruction in combinedList)
        {
            var possibleCommandGO = Instantiate(possibleCommandPrefab) as GameObject;
            possibleCommandGO.transform.SetParent(possibleCommandsContainer.transform, false);

            if (instruction != string.Empty)
            {
                possibleCommandGO.GetComponent<Text>().text = instruction;
                possibleCommandGO.GetComponent<PossibleCommandOnClick>().SetupPossibleCommand(codeInputField, instruction);
            }
            else
                possibleCommandGO.GetComponent<PossibleCommandOnClick>().enabled = false;
        }
    }

    private void HighlightCode()
    {
        if (robot.IsStarted)
        {
            if (robot.CurrentInstructionIndexIsValid)
                _formattedInstructions[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, "#D5A042FF", _formattedInstructions[robot.CurrentInstructionIndex]);
            else
            {
                _formattedInstructions[robot.CurrentInstructionIndex] = ColorTextOnCondition(true, Color.red, _formattedInstructions[robot.CurrentInstructionIndex]);
                SetFeedbackText(ColorTextOnCondition(true, Color.red, robot.Feedback), 0);
            }

            codeOutputField.text = string.Join("\n", _formattedInstructions.ToArray());
        }
    }

    private void UpdateRobotInfoLabels()
    {
        //If robot has no instructions at this time, get instructions from CodeInputField.
        _formattedInstructions = _formattedInstructions.Count > 0 ? _formattedInstructions : codeInputField.text.Split('\n').ToList();

        inventoryLabel.text = "INVENTORY (" + robot.Inventory.Count + "/" + robot.Settings_InventoryCapacity() + ")";
        memoryText.text = ColorTextOnCondition(_formattedInstructions.Count > robot.Settings_Memory(), Color.red, string.Format("MEMORY: {0}/{1}", _formattedInstructions.Count, robot.Settings_Memory()));
        energyText.text = ColorTextOnCondition(robot.Energy < 4, Color.red, string.Format("ENERGY: {0}/{1}", robot.Energy, robot.Settings_MaxEnergy()));
        healthText.text = string.Format("HEALTH: {0}/{1}", robot.Health, robot.Settings_StartHealth());
        damageText.text = string.Format("DAMAGE: {0}", robot.Settings_Damage());
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

}
