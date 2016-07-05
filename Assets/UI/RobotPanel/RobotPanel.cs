using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class RobotPanel : MonoBehaviour
{
    public Text titleText;

    public Text feedbackText;

    public Text memoryText;
    public Text energyText;

    public Button runButton;
    public Button closeButton;

    public Text codeInputLabel;
    public InputField codeInputField;
    public GameObject codeInputPanel;

    public Text codeOutputLabel;
    public Text codeOutputField;
    public GameObject codeOutputPanel;

    public Text helpTextLabel;
    public Text helpTextText;
    public GameObject helpTextPanel;

    public Text inventoryLabel;
    public GameObject inventoryPanel;
    public GameObject inventoryContainer;
    public GameObject ironPrefab;
    public GameObject copperPrefab;

    public static RobotPanel instance;

    private Animator animator;
    private Robot robot;
    private List<string> instructionListCopy;
    private IEnumerator feedbackClearCoroutine;

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

        Robot.OnInventoryChanged += InventoryUpdated;
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

            instructionListCopy = robot.GetInstructions().Select(instruction => instruction.ToString()).ToList();

            if (robot.IsStarted)
            {
                if (robot.InstructionBeingExecutedIsValid)
                    instructionListCopy[robot.InstructionBeingExecuted] = "<color=#D5A042FF>" + instructionListCopy[robot.InstructionBeingExecuted] + "</color>";
                else {
                    instructionListCopy[robot.InstructionBeingExecuted] = "<color=red>" + instructionListCopy[robot.InstructionBeingExecuted] + "</color>";
                    SetFeedbackText("<color=red>" + robot.Feedback + "</color>", 0);
                }
                codeOutputField.text = string.Join("\n", instructionListCopy.ToArray());

                inventoryLabel.text = "INVENTORY (" + robot.Inventory.Count + "/" + robot.Settings_InventoryCapacity() + ")";
            }

            var instructions = instructionListCopy.Count > 0 ? instructionListCopy : codeInputField.text.Split('\n').ToList();
            bool memoryExceeded = instructions.Count > robot.Settings_Memory();
            string colorPrefix = memoryExceeded ? "<color=red>" : "";
            string colorSuffix = memoryExceeded ? "</color>" : "";
            memoryText.text = colorPrefix + "MEMORY: " + instructions.Count + "/" + robot.Settings_Memory() + colorSuffix;
            energyText.text = string.Format("ENERGY: {0}/{1}", robot.Energy, robot.Settings_MaxEnergy());
        }
    }

    public void ShowPanel(Robot robot)
    {
        this.robot = robot;
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
        List<string> instructions = codeInputField.text.Split('\n').ToList();

        robot.RunCode(instructions);

        if (robot.IsStarted)
            EnableRunnningModePanel();
    }

    public void ClosePanel()
    {
        KeyboardManager.KeyboardLockOff();
        robot = null;
        animator.Play("RobotMenuSlideOut");
    }

    private void CodeInputToUpper(string arg0 = "")
    {
        codeInputField.text = codeInputField.text.ToUpper();
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

    private void InventoryUpdated(Robot robot)
    {
        if (this.robot != null && robot == this.robot)
        {
            Debug.Log("InventoryUpdated: " + this.robot.Inventory);

            foreach (Transform child in inventoryContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

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

    private void EnableSetupModePanel()
    {
        titleText.text = "HARVESTER SETUP";        

        helpTextPanel.SetActive(true);
        helpTextText.text = string.Join("\n", robot.commonInstructions.ToArray());
        helpTextText.text += "\n\n";
        helpTextText.text += string.Join("\n", robot.GetSpecializedInstruction().ToArray());

        codeInputPanel.SetActive(true);
        codeInputField.onValueChanged.AddListener(KeyboardManager.KeyboardLockOn);
        codeInputField.onValueChanged.AddListener(CodeInputToUpper);
        codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);
        codeInputField.text = robot.GetDemoInstructions();  /* Pre filled demo data */

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(RunCode);

        inventoryPanel.SetActive(false);
        codeOutputPanel.SetActive(false);
    }

    private void EnableRunnningModePanel()
    {
        titleText.text = "HARVESTER";
        InventoryUpdated(robot);

        codeOutputPanel.SetActive(true);
        inventoryPanel.SetActive(true);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);

        codeInputPanel.SetActive(false);
        helpTextPanel.SetActive(false);
    }
}

